Param(
    [string]$_branchName, 
    [string]$GITHUB_TOKEN
)

$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
$ErrorActionPreference = 'stop'

function Main {
    try {
        Import-Module $_boltOnModulePath -Force
        LogBeginFunction "$($MyInvocation.MyCommand.Name)"
        dotnet tool install nukeeper --global

        nukeeper update --include=^BoltOn --source=https://api.nuget.org/v3/index.json
        docker-compose up -d --build
        Start-Sleep -s 10

        $appSettingsPath = Join-Path $_scriptDirPath "BoltOn.Sample.WebApi"
        $appSettingsPath = Join-Path $appSettingsPath "appsettings.json"
        $appSettingsFile = Get-Content $appSettingsPath -raw | ConvertFrom-Json
        $appSettingsFile.update $_.IsIntegrationTestsEnabled="true"
        $appSettingsFile | ConvertTo-Json -depth 32| set-content $appSettingsPath

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

