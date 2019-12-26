Param([string]$branchName)

$_branchName = $branchName
$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"

function Main
{
    Import-Module $_boltOnModulePath -Force
    LogDebug "Branch: $_branchName"
    BuildAndTest
}

function BuildAndTest
{
    LogDebug "Building solution..."
    dotnet build --configuration Release
    LogInfo "Built"
    dotnet test --configuration Release
    LogInfo "Executed Tests"
}

Main