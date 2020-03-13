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
        if ($_branchName -eq "master") {
            nukeeper repo "https://github.com/gokulm/BoltOn/" $GITHUB_TOKEN --fork=SingleRepositoryOnly `
                --source=https://api.nuget.org/v3/index.json --include=^BoltOn --consolidate --targetBranch=master
        }
        else {
            Set-Location -Path .
            nukeeper update samples --include=^BoltOn --source=https://api.nuget.org/v3/index.json
            $currentLocation = Get-Location
            $samplesDirPath = Join-Path $currentLocation "samples"
            Set-Location $samplesDirPath
            docker-compose up -d --build
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

