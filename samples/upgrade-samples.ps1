Param(
    [string]$_branchName, 
    [string]$GITHUB_TOKEN
)

$_scriptDirPath = $PSScriptRoot
$ErrorActionPreference = 'stop'

function Main {
    try {
        $parentDirPath = Split-Path -parent $_scriptDirPath
        $buildDirPath = Join-Path $parentDirPath "build"
        $_boltOnModulePath = Join-Path $buildDirPath "bolton.psm1"

        Import-Module $_boltOnModulePath -Force
        LogBeginFunction "$($MyInvocation.MyCommand.Name)"

        dotnet tool install nukeeper --global
        nukeeper update --include=^BoltOn --source=https://api.nuget.org/v3/index.json
        docker-compose down 
        docker-compose up -d --build
        Start-Sleep -s 60

        $appSettingsPath = Join-Path $_scriptDirPath "BoltOn.Samples.Tests"
        $appSettingsPath = Join-Path $appSettingsPath "appsettings.json"
        $appSettingsFile = Get-Content $appSettingsPath -raw | ConvertFrom-Json
        $appSettingsFile.IsIntegrationTestsEnabled=$true
        $appSettingsFile | ConvertTo-Json | Set-Content $appSettingsPath

        Build
        Test

        if ($_branchName -eq "master") {
            nukeeper repo "https://github.com/gokulm/BoltOn/" $GITHUB_TOKEN --fork=SingleRepositoryOnly `
                --source=https://api.nuget.org/v3/index.json --include=^BoltOn --consolidate --targetBranch=master
        }
        LogEndFunction "$($MyInvocation.MyCommand.Name)"
    }
    catch {
        LogError $_.Exception.Message
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
    }
}

Main

