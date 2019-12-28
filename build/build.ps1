Param([string]$branchName, [string]$nugetApiKey)

$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"

function Main
{
    Import-Module $_boltOnModulePath -Force
    Write-BeginFunction "$($MyInvocation.MyCommand.Name)"
    Write-Debug "Branch: $branchName"
    Write-Debug "NuGet API Key: $nugetApiKey"
    if ($branchName)  {
        $changedFiles = git diff "origin/$_branchName...HEAD" --no-commit-id --name-only
        $changedFiles = git diff --no-commit-id --name-only
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

            Write-Debug "All the changed projects:"
            foreach ($changed in $changedProjects) {
                Write-Debug $changed.Project
            }
        }

        foreach ($changedFile in $changedFiles) {
            Write-Debug $changedFile
        }

        $commits = git log -n 3 --pretty=%B
        foreach ($commit in $commits) {
            Write-Debug $commit
        }
    } 

    Update-AssemblyVersion './src/BoltOn/AssemblyInfo.cs' 0.8.3
    Update-PackageVersion './src/BoltOn/BoltOn.csproj' 0.8.3
    BuildAndTest
    Write-EndFunction "$($MyInvocation.MyCommand.Name)"
}

function BuildAndTest
{
    Write-BeginFunction "$($MyInvocation.MyCommand.Name)"
    dotnet build --configuration Release
    Write-Debug "Built"
    dotnet test --configuration Release
    Write-EndFunction "$($MyInvocation.MyCommand.Name)"
}

Main