Param(
    [string]$GITHUB_ACTOR
)

$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
$ErrorActionPreference = 'stop'

function Main {
    try {
        Import-Module $_boltOnModulePath -Force
        LogDebug "Actor: $GITHUB_ACTOR"
        LogBeginFunction "$($MyInvocation.MyCommand.Name)"
        Build
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

Main