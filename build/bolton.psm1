$_allowedCommitTypes = "fix", "feat", "fix!", "feat!"
$_srcDirPath = "src/"

function LogError([string]$message) {
    Write-Host "$message" -ForegroundColor Red
}

function LogWarning([string]$message) {
    Write-Host "$message" -ForegroundColor Yellow
}

function LogInfo([string]$message) {
    Write-Host "$message" -ForegroundColor DarkGreen
}

function LogDebug([string]$message) {
    Write-Host "$message" -ForegroundColor DarkCyan
}

function LogBeginFunction([string]$message) {
    LogInfo "== BEGIN $message =="
}

function LogEndFunction([string]$message) {
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
        [string]$version
    )

    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    if (!(Test-Path $csprojFilePath)) {
        throw "File not found: $csprojFilePath"
    }

    $xml = New-Object XML
    $xml.Load($csprojFilePath)
    $xml.Project.PropertyGroup[0].Version = $version
    $xml.Save($csprojFilePath)
    LogDebug "Updated version to $version"
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function GetProjectNewVersions {
    param (
        [Parameter(Mandatory=$true)][string]$commitMessage,
        [Parameter(Mandatory=$false)][string]$changedProjects
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
        Validate $match.Success "Invalid commit message"
    }

    $type = $match.Groups['type']
    $scope = $match.Groups['scope']
    $subject = $match.Groups['subject']
    Validate ($type -and $subject) "Type or subject not found in commit message"
    Validate ($_allowedCommitTypes | Where-Object { $type -like $_ }) "Invalid commit type"
    $isBreakingChange = $type -match "\!$"
    $projectVersions = @{};

    if(-Not([string]::IsNullOrEmpty($scope)))
    {
        $allowedScopes = Get-ChildItem $_srcDirPath -Name -attributes D 
        $scopes = $scope.ToString().Split(",", $option)
        foreach ($tempScope in $scopes) {
            $tempScope = $tempScope.Trim()
            Validate ($allowedScopes | Where-Object { $tempScope -like $_ }) "Invalid scope"
            $newVersion = Versionize $tempScope $type $isBreakingChange
            $projectVersions[$tempScope] = $newVersion
        }
    }
    else {
        Validate $changedProjects "Changed projects is required as scope is not part of commit message"

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
    $currentVersion =  New-Object System.Version ( $nugetPackageLatestVersion )
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

function Validate {
    param (
        [string]$isValid,
        [string]$exceptionMessage
    )
    
    if (-Not($isValid)) {
        throw $exceptionMessage
    }
}

export-modulemember -function LogError, LogWarning, LogDebug, GetNugetPackageLatestVersion, `
    UpdateAssemblyVersion, UpdateVersion, LogBeginFunction, LogEndFunction, `
    GetProjectNewVersions
    