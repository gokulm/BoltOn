Param([string]$branchName, [string]$nugetApiKey)

$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"

function Main
{
    Import-Module $_boltOnModulePath -Force
    Log-Debug "Branch: $branchName"
    Log-Debug "NuGet API Key: $nugetApiKey"
    if ($branchName)  {
        $changedFiles = git diff "origin/$_branchName...HEAD" --no-commit-id --name-only
        Log-Info "Files Changed: $changedFiles"

        $commits = git log -n 3 --pretty=%B
        foreach ($commit in $commits) {
            Log-Info $commit
        }
    } 

    Update-AssemblyVersion './src/BoltOn/AssemblyInfo.cs' 0.8.3
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