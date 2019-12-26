Param([string]$branchName)

$_branchName = $branchName
$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
$_changedFiles = ''

function Main
{
    Import-Module $_boltOnModulePath -Force
    LogDebug "Branch: $_branchName"
    if ($_branchName)  {
        $global:_changedFiles = git diff "origin/$_branchName...HEAD" --no-commit-id --name-only
        LogInfo "Files Changed: $_changedFiles"

        $commits = git log -n 5
        LogInfo "Commits: $commits"
    }

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