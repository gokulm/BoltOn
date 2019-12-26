Param([string]$branchName)

$_branchName = $branchName
$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
$_changedFiles = ''

function Main
{
    Import-Module $_boltOnModulePath -Force
    Log-Debug "Branch: $_branchName"
    if ($_branchName)  {
        $_changedFiles = git diff "origin/$_branchName...HEAD" --no-commit-id --name-only
        Log-Info "Files Changed: $_changedFiles"
    } 
    
    $commits = git log -n 5 --pretty=%B
    foreach ($commit in $commits) {
        Log-Info $commit
    }

    # Update-AssemblyInfo './src/BoltOn/'
    # Update-CsProjVersion './src/BoltOn/BoltOn.csproj' 1.0.0
    # BuildAndTest
}

function BuildAndTest
{
    Log-Debug "Building solution..."
    dotnet build --configuration Release
    Log-Info "Built"
    dotnet test --configuration Release
    Log-Info "Executed Tests"
}

Main