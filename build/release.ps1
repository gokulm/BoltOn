Param([string]$branchName, [string]$nugetApiKey)

$_scriptDirPath = $PSScriptRoot
$_rootDirPath = Split-Path $_scriptDirPath
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
# $_allowedScopes = "BoltOn", "BoltOn.Data.EF", "BoltOn.Data.CosmosDb", "BoltOn.Bus.MassTransit"

function Main {
    Import-Module $_boltOnModulePath -Force
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    LogDebug "Branch: $branchName"
    # BuildAndTest

    if ($branchName) {
        $changedFiles = git diff "origin/$branchName...HEAD" --no-commit-id --name-only
        if ($changedFiles.Length -gt 0) {
            $tempChangedProjects = $changedFiles | Where-Object { $_.ToString().StartsWith("src/", 1) } | Select-Object `
            @{
                N = 'Project';
                E = 
                { 
                    $temp = $_.ToString().Substring(4);
                    $temp.Substring(0, $temp.IndexOf("/")) 
                }
            } -Unique

            $changedProjects = $tempChangedProjects | Select-Object -ExpandProperty Project
            $changedProjects
            $commits = git log -n 1 --pretty=%B
            $newVersions = GetProjectNewVersions $commits[0] $changedProjects 
            # $newVersions = GetProjectNewVersions "feat(BoltOn, BoltOn.Data.EF): test" 
            $newVersions
            $outputPath = Join-Path $_rootDirPath "publish"
            foreach ($key in $newVersions.keys) {
                $projectPath = Join-Path $_rootDirPath "src/$($key)/$($key).csproj"
                UpdateVersion $projectPath  $newVersions[$key]
                dotnet pack $projectPath --configuration Release -o $outputPath
            }
        }
    } 

    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function BuildAndTest {
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    dotnet build --configuration Release
    LogDebug "Built"
    dotnet test --configuration Release
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

Main