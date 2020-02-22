Param(
    [string]$GITHUB_ACTOR
)

$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
$ErrorActionPreference = 'stop'

function Main {
    try {
        LogDebug "Actor: $GITHUB_ACTOR"
        Import-Module $_boltOnModulePath -Force
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