$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
$ErrorActionPreference = 'stop'

function Main {
    try {
        Import-Module $_boltOnModulePath -Force
        LogBeginFunction "$($MyInvocation.MyCommand.Name)"
        dotnet tool install dnt --global --add-source=https://api.nuget.org/v3/index.json
        $parentDirPath = Split-Path -parent $_scriptDirPath
        $samplesDirPath = Join-Path $parentDirPath "samples"
        Set-Location $samplesDirPath
        dnt switch-to-projects switcher.json
        # docker-compose up -d --build
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

