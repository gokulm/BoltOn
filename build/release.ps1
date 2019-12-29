Param([string]$branchName, [string]$nugetApiKey)

$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"

function Main
{
    Import-Module $_boltOnModulePath -Force
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    LogDebug "Branch: $branchName"
    LogDebug "NuGet API Key: $nugetApiKey"
    if ($branchName)  
    {
        $changedFiles = git diff "origin/$branchName...HEAD" --no-commit-id --name-only
        if($changedFiles.Length -gt 0)
        {
            $changedProjects = $changedFiles | Where-Object { $_.ToString().StartsWith("src/", 1) } | Select-Object `
            @{
                N='Project';
                E= 
                { 
                    $temp = $_.ToString().Substring(4);
                    $temp.Substring(0, $temp.IndexOf("/")) 
                }
            } -Unique

            LogDebug "All the changed projects:"
            foreach ($changed in $changedProjects) {
                LogDebug $changed.Project
            }
        }

        foreach ($changedFile in $changedFiles) {
            LogDebug $changedFile
        }

        $commits = git log -n 3 --pretty=%B
        foreach ($commit in $commits) {
            LogDebug $commit
        }
    } 

    Update-AssemblyVersion './src/BoltOn/AssemblyInfo.cs' 0.8.3
    Update-PackageVersion './src/BoltOn/BoltOn.csproj' 0.8.3
    BuildAndTest
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function BuildAndTest
{
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    dotnet build --configuration Release
    LogDebug "Built"
    dotnet test --configuration Release
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

Main