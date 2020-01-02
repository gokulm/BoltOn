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
        [parameter(Mandatory)]$csprojFilePath,
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

function ParseConventionalCommitMessage {
    param (
        [parameter(Mandatory)]$commitMessage,
        [parameter(Mandatory)]$allowedCommitTypes,
        [parameter(Mandatory)]$allowedScopes
    )

    $option = [System.StringSplitOptions]::RemoveEmptyEntries
    $separator = "\r\n", "\r", "\n"
    $commitMessageLines = $commitMessage.Split($separator, $option);
    $header = $commitMessageLines[0]
    LogDebug "Commit header: $header"

    $match = [regex]::Match($header, '^(?<type>.*)\((?<scope>.*)\): (?<subject>.*)$')
    Validate $match.Success "Invalid commit message"

    $type = $match.Groups['type']
    $scope = $match.Groups['scope']
    $subject = $match.Groups['subject']
    Validate ($type -and $scope -and $subject) "Type or scope or subject not found in commit message"
    Validate ($allowedCommitTypes | Where-Object { $type -like $_ }) "Invalid commit type"
    $scopes = $scope.ToString().Split(",", $option)
    foreach ($tempScope in $scopes) {
        Validate ($allowedScopes | Where-Object { $tempScope.Trim() -like $_ }) "Invalid scope"
    }
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
    ParseConventionalCommitMessage
    