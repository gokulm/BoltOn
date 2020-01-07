$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"

function Main
{
    Import-Module $_boltOnModulePath -Force
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    BuildAndTest
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

Main