function Log-Error([string]$message)
{
	Write-Host "$message" -ForegroundColor Red
}

function Log-Warning([string]$message)
{
    Write-Host "$message" -ForegroundColor Yellow
}

function Log-Info([string]$message)
{
    Write-Host "*********** $message ***********" -ForegroundColor DarkGreen
}

function Log-Debug([string]$message)
{
    Write-Host "$message" -ForegroundColor DarkCyan
}

function Get-NugetPackageLatestVersion()
{ 
    param(
        [parameter(Mandatory)]$packageName
    )
    return Find-Package $packageName | Select-Object -ExpandProperty Version -first 1
}

function Update-AssemblyVersion() 
{
    param(
        [parameter(Mandatory)]$assemblyInfoFilePath,
        [parameter(Mandatory)]$version
    )
    if (!(Test-Path $assemblyInfoFilePath)) {
        throw "File not found: $assemblyInfoFilePath"
    }
    $tempVersion = New-Object System.Version ($version)
    $newVersion = new-object System.Version ($tempVersion.Major, $tempVersion.Minor, $tempVersion.Build, 0)
    $content = Get-Content $assemblyInfoFilePath
    $result = $content -creplace 'Version\("([^"]*)', "Version(""$newVersion"
    Set-Content $assemblyInfoFilePath $result
    Log-Info "Updated assembly version to $newVersion"
}

function Update-PackageVersion()
{
    param(
        [parameter(Mandatory)]$csprojFilePath,
        [string]$version
    )

    if(!(Test-Path $csprojFilePath))
    {
        throw "File not found: $csprojFilePath"
    }

    $xml = New-Object XML
    $xml.Load($csprojFilePath)
    $xml.Project.PropertyGroup[0].PackageVersion = $version
    $xml.Save($csprojFilePath)
    Log-Info "Updated package version to $version"
}

export-modulemember -function Log-Error, Log-Warning, Log-Info, Log-Debug, Get-NugetPackageLatestVersion, `
    Update-AssemblyVersion, Update-PackageVersion