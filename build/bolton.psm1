$_allowedCommitTypes = "fix", "feat", "fix!", "feat!"
$_srcDirPath = "src/"
$_nugetSource = "https://api.nuget.org/v3/index.json"

function LogError() {
    param(
        [parameter(Mandatory)]$message
    )
    Write-Host "$message" -ForegroundColor Red
}

function LogWarning() {
    param(
        [parameter(Mandatory)]$message
    )
    Write-Host "$message" -ForegroundColor Yellow
}

function LogInfo() {
    param(
        [parameter(Mandatory)]$message
    )
    Write-Host "$message" -ForegroundColor DarkGreen
}

function LogDebug() {
    param(
        [parameter(Mandatory)]$message
    )
    Write-Host "$message" -ForegroundColor DarkCyan
}

function LogBeginFunction() {
    param(
        [parameter(Mandatory)]$message
    )
    LogInfo "== BEGIN $message =="
}

function LogEndFunction() {
    param(
        [parameter(Mandatory)]$message
    )
    LogInfo "== END $message =="
}

function GetNugetPackageLatestVersion() { 
    param(
        [parameter(Mandatory)]$packageName
    )
    return Find-Package $packageName | Select-Object -ExpandProperty Version -first 1
}

function UpdateAssemblyVersion() {
    param(
        [parameter(Mandatory)]$assemblyInfoFilePath,
        [parameter(Mandatory)]$version
    )
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    if (!(Test-Path $assemblyInfoFilePath)) {
        throw "File not found: $assemblyInfoFilePath"
    }
    $tempVersion = New-Object System.Version ($version)
    $newVersion = New-Object System.Version ($tempVersion.Major, $tempVersion.Minor, $tempVersion.Build, 0)
    $content = Get-Content $assemblyInfoFilePath
    $result = $content -creplace 'Version\("([^"]*)', "Version(""$newVersion"
    Set-Content $assemblyInfoFilePath $result
    LogDebug "Updated assembly version to $newVersion"
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function UpdateVersion() {
    param(
        [parameter(Mandatory)][string]$csprojFilePath,
        [parameter(Mandatory)][string]$version
    )

    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    if (!(Test-Path $csprojFilePath)) {
        throw "File not found: $csprojFilePath"
    }

    $xml = New-Object XML
    $xml.Load($csprojFilePath)
    $xml.Project.PropertyGroup[0].Version = $version
    $xml.Save($csprojFilePath)
    CheckLastExitCode "Updating csproj version failed"
    
    LogDebug "Updated $csprojFilePath version to $version"
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function GetProjectNewVersions {
    param (
        [string]$commitMessage,
        [string[]]$changedProjects
    )

    $option = [System.StringSplitOptions]::RemoveEmptyEntries
    $match = GetConventionalCommitMessage $commitMessage
    ThrowIfNotValid $match.Success "Invalid commit message"
    $type = $match.Groups['type']
    $scope = $match.Groups['scope']
    $subject = $match.Groups['subject']
    ThrowIfNotValid ($type -and $subject) "Type or subject not found in commit message"
    $type = $type.ToString().Trim();
    ThrowIfNotValid ($_allowedCommitTypes | Where-Object { $type -like $_ }) "Invalid commit type"
    $isBreakingChange = $type -match "\!$"
    $projectVersions = @{}

    RegisterNuGetPackageSource
    CheckLastExitCode "RegisterNuGetPackageSource failed"

    if(-Not([string]::IsNullOrEmpty($scope)))
    {
        $allowedScopes = Get-ChildItem $_srcDirPath -Name -attributes D 
        $scopes = $scope.ToString().Split(",", $option)
        foreach ($tempScope in $scopes) {
            $tempScope = $tempScope.Trim()
            ThrowIfNotValid ($allowedScopes | Where-Object { $tempScope -like $_ }) "Invalid scope"
            $newVersion = Versionize $tempScope $type $isBreakingChange
            $projectVersions[$tempScope] = $newVersion
        }
    }
    else {
        foreach($changedProject in $changedProjects){
            $newVersion = Versionize $changedProject $type $isBreakingChange
            $projectVersions[$changedProject] = $newVersion
        }
    }

    return $projectVersions
}

function Versionize()
{
    param(
        [Parameter(Mandatory=$true)][string]$project,
        [Parameter(Mandatory=$true)][string]$commitType,
        [Parameter(Mandatory=$true)][bool]$isBreakingChange
    )

    $nugetPackageLatestVersion = (GetNugetPackageLatestVersion $project)
    $currentVersion =  New-Object System.Version($nugetPackageLatestVersion)
    $newVersion = $null

    if($isBreakingChange)
    {
        $newVersion = New-Object System.Version (($currentVersion.Major + 1), 0, 0)
        LogDebug "Project: $project New version: $newVersion"
        return $newVersion
    }

    switch($commitType)
    {
        "feat" { $newVersion = New-Object System.Version ($currentVersion.Major, ($currentVersion.Minor + 1), 0); break; }
        "fix" {  $newVersion = New-Object System.Version ($currentVersion.Major, $currentVersion.Minor, ($currentVersion.Build + 1)); break; }
        default { throw "Invalid commit type: $commitType" }
    }

    LogDebug "Project: $project New version: $newVersion"
    return $newVersion

}

function ThrowIfNotValid {
    param (
        [string]$isValid,
        [string]$exceptionMessage
    )
    
    if (-Not($isValid)) {
        $LASTEXITCODE = 1
        throw $exceptionMessage
    }
}

# this method call can be removed once GitHub Action PowerShell supports NuGet v3
function RegisterNuGetPackageSource {
    $packageSources = Get-PackageSource
    if(@($packageSources).Where{$_.location -eq $_nugetSource}.count -eq 0)
    {
        Register-PackageSource -Name MyNuGet -Location $_nugetSource -ProviderName NuGet
    }
}

function CheckLastExitCode([string]$exceptionMessage)
{
	if($LASTEXITCODE -ne 0)
	{
        throw $exceptionMessage
	}
}

function Build {
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    dotnet build --configuration Release
    CheckLastExitCode "dotnet build failed"
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function GetConventionalCommitScope {
    param (
        [Parameter(Mandatory=$true)]$commitMessage
    )
    $match = GetConventionalCommitMessage $commitMessage
    if($match.Success)
    {
        return $match.Groups['scope']
    }
    return $null
}

function Test {
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    dotnet test --no-build --no-restore --configuration Release --verbosity normal
    CheckLastExitCode "test(s) failed"
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function GetConventionalCommitMessage {
    param (
        [string]$commitMessage
    )

    $option = [System.StringSplitOptions]::RemoveEmptyEntries
    $separator = "\r\n", "\r", "\n"
    $commitMessageLines = $commitMessage.Split($separator, $option);
    $header = $commitMessageLines[0]
    LogDebug "Commit header: $header"

    # to support commit messages w/ and w/o scopes
    # if scope is present, only scoped projects will be versioned, else, all the changed projects
    $match = [regex]::Match($header, '^(?<type>.*)\((?<scope>.*)\): (?<subject>.*)$')
    if(-Not($match.Success))
    {
        $match = [regex]::Match($header, '^(?<type>.*): (?<subject>.*)$')
    }
    
    return $match
}

export-modulemember -function LogError, LogWarning, LogDebug, LogInfo, GetNugetPackageLatestVersion, `
    UpdateAssemblyVersion, UpdateVersion, LogBeginFunction, LogEndFunction, `
    GetProjectNewVersions, CheckLastExitCode, Build, Test, GetConventionalCommitScope