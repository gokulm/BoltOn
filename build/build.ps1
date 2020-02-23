Param(
    [string]$GITHUB_ACTOR,
    [string]$GITHUB_TOKEN
)

$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
$ErrorActionPreference = 'stop'

function Main {
    try {
        Import-Module $_boltOnModulePath -Force
        LogDebug "Actor: $GITHUB_ACTOR"
        if($null -ne $GITHUB_TOKEN)
        {
            LogDebug "token found: $GITHUB_TOKEN"
        }
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