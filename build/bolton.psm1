function Write-Error([string]$message)
{
	Write-Host "$message" -ForegroundColor Red
}

function Write-Warning([string]$message)
{
    Write-Host "$message" -ForegroundColor Yellow
}

function Write-Info([string]$message)
{
    Write-Host "$message" -ForegroundColor DarkGreen
}

function Write-Debug([string]$message)
{
    Write-Host "$message" -ForegroundColor DarkCyan
}

function Write-BeginFunction([string]$message)
{
	Write-Info "== BEGIN $message =="
}

function Write-EndFunction([string]$message)
{
	Write-Info "== END $message =="
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
    Write-BeginFunction "$($MyInvocation.MyCommand.Name)"
    if (!(Test-Path $assemblyInfoFilePath)) {
        throw "File not found: $assemblyInfoFilePath"
    }
    $tempVersion = New-Object System.Version ($version)
    $newVersion = new-object System.Version ($tempVersion.Major, $tempVersion.Minor, $tempVersion.Build, 0)
    $content = Get-Content $assemblyInfoFilePath
    $result = $content -creplace 'Version\("([^"]*)', "Version(""$newVersion"
    Set-Content $assemblyInfoFilePath $result
    Write-Debug "Updated assembly version to $newVersion"
    Write-EndFunction "$($MyInvocation.MyCommand.Name)"
}

function Update-PackageVersion()
{
    param(
        [parameter(Mandatory)]$csprojFilePath,
        [string]$version
    )

    Write-BeginFunction "$($MyInvocation.MyCommand.Name)"
    if(!(Test-Path $csprojFilePath))
    {
        throw "File not found: $csprojFilePath"
    }

    $xml = New-Object XML
    $xml.Load($csprojFilePath)
    $xml.Project.PropertyGroup[0].PackageVersion = $version
    $xml.Save($csprojFilePath)
    Write-Debug "Updated package version to $version"
    Write-EndFunction "$($MyInvocation.MyCommand.Name)"
}

export-modulemember -function Write-Error, Write-Warning, Write-Debug, Get-NugetPackageLatestVersion, `
    Update-AssemblyVersion, Update-PackageVersion, Write-BeginFunction, Write-EndFunction