Param([string]$branchName, [string]$nugetApiKey)

$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
$_changedFiles = ''

function Main
{
    Import-Module $_boltOnModulePath -Force
    Log-Debug "Branch: $branchName"
    Log-Debug "NuGet API Key: $nugetApiKey"
    if ($_branchName)  {
        $_changedFiles = git diff "origin/$_branchName...HEAD" --no-commit-id --name-only
        Log-Info "Files Changed: $_changedFiles"

        $commits = git log -n 3 --pretty=%B
        foreach ($commit in $commits) {
            Log-Info $commit
        }
    } 

    Update-AssemblyVersion './src/BoltOn/AssemblyInfo.cs' $nugetApiKey
    Update-PackageVersion './src/BoltOn/BoltOn.csproj' 0.8.3
    BuildAndTest
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