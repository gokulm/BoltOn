$_scriptDirPath = $PSScriptRoot
$parentDirPath = Split-Path -parent $_scriptDirPath
$buildDirPath = Join-Path $parentDirPath "build"
$_boltOnModulePath = Join-Path $buildDirPath "bolton.psm1"
$ErrorActionPreference = 'stop'

function Main {
    try {
        Import-Module $_boltOnModulePath -Force
        LogBeginFunction "$($MyInvocation.MyCommand.Name)"

        SwitchPackagesToProjects
        DockerCompose
        ExecuteApp "BoltOn.Samples.Console"
        ExecuteApp "BoltOn.Samples.WebApi"
        EnableIntegrationTests
        Build
        Start-Sleep -s 60
        Test
        
        LogEndFunction "$($MyInvocation.MyCommand.Name)"
    }
    catch {
        LogError $_.Exception.Message
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
    }
}

function SwitchPackagesToProjects
{
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    dotnet tool install dnt --global --add-source=https://api.nuget.org/v3/index.json
    dnt switch-to-projects switcher.json
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function DockerCompose
{
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    docker-compose down 
    docker-compose -f docker-compose-local.yml up -d
    Start-Sleep -s 60
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function ExecuteApp()
{
    param(
        [parameter(Mandatory)]$projectName
    )

    LogDebug "Starting $projectName..."
    $projectDirPath = Join-Path $_scriptDirPath $projectName 
    $projectPath = Join-Path $projectDirPath "$projectName.csproj"
    $projectPath
    Start-Process -FilePath 'dotnet' -ArgumentList "run --project $projectPath"
    LogDebug "Started $projectName"
}

function EnableIntegrationTests
{
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    $appSettingsPath = Join-Path $_scriptDirPath "BoltOn.Samples.Tests"
    $appSettingsPath = Join-Path $appSettingsPath "appsettings.json"
    $appSettingsFile = Get-Content $appSettingsPath -raw | ConvertFrom-Json
    $appSettingsFile.IsIntegrationTestsEnabled=$true
    $appSettingsFile | ConvertTo-Json | Set-Content $appSettingsPath
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

Main

