Param([string]$branchName)

$_branchName = $branchName
$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"

function Main
{
    LogDebug $_branchName
    Import-Module $_boltOnModulePath -Force
    BuildAndTest
}

function BuildAndTest
{
    LogDebug "About to build the solution..."
    dotnet build --configuration Release
    LogInfo "Built"
    # dotnet test --configuration Release
}

Main