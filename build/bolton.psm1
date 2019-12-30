function LogError([string]$message)
{
	Write-Host "$message" -ForegroundColor Red
}

function LogWarning([string]$message)
{
    Write-Host "$message" -ForegroundColor Yellow
}

function LogInfo([string]$message)
{
    Write-Host "$message" -ForegroundColor DarkGreen
}

function LogDebug([string]$message)
{
    Write-Host "$message" -ForegroundColor DarkCyan
}

function LogBeginFunction([string]$message)
{
	LogInfo "== BEGIN $message =="
}

function LogEndFunction([string]$message)
{
	LogInfo "== END $message =="
}

function GetNugetPackageLatestVersion()
{ 
    param(
        [parameter(Mandatory)]$packageName
    )
    return Find-Package $packageName | Select-Object -ExpandProperty Version -first 1
}

function UpdateAssemblyVersion() 
{
    param(
        [parameter(Mandatory)]$assemblyInfoFilePath,
        [parameter(Mandatory)]$version
    )
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    if (!(Test-Path $assemblyInfoFilePath)) {
        throw "File not found: $assemblyInfoFilePath"
    }
    $tempVersion = New-Object System.Version ($version)
    $newVersion = new-object System.Version ($tempVersion.Major, $tempVersion.Minor, $tempVersion.Build, 0)
    $content = Get-Content $assemblyInfoFilePath
    $result = $content -creplace 'Version\("([^"]*)', "Version(""$newVersion"
    Set-Content $assemblyInfoFilePath $result
    LogDebug "Updated assembly version to $newVersion"
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function UpdateVersion()
{
    param(
        [parameter(Mandatory)]$csprojFilePath,
        [string]$version
    )

    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    if(!(Test-Path $csprojFilePath))
    {
        throw "File not found: $csprojFilePath"
    }

    $xml = New-Object XML
    $xml.Load($csprojFilePath)
    $xml.Project.PropertyGroup[0].Version = $version
    $xml.Save($csprojFilePath)
    LogDebug "Updated version to $version"
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function ParseCommitMessage {
    param (
        [parameter(Mandatory)]$commitMessage
    )

    $commitMessageLines = commit.Message.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );

                foreach ($changed in $commitMessageLines) {
                    LogDebug $changed
                }
    
}

export-modulemember -function LogError, LogWarning, LogDebug, GetNugetPackageLatestVersion, `
    UpdateAssemblyVersion, UpdateVersion, LogBeginFunction, LogEndFunction, ParseCommitMessage
    