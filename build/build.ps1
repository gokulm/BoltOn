$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
$ErrorActionPreference = 'stop'

function Main {
    try {
        Import-Module $_boltOnModulePath -Force
        LogBeginFunction "$($MyInvocation.MyCommand.Name)"
        BuildAndTest
        LogEndFunction "$($MyInvocation.MyCommand.Name)"
    }
    catch {
        LogError $_.Exception.Message
        if ($LastExitCode -ne 0) {
            exit $LastExitCode
        }
    }
}

Main