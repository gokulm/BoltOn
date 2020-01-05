Param([string]$branchName, [string]$nugetApiKey)

$_scriptDirPath = $PSScriptRoot
$_rootDirPath = Split-Path $_scriptDirPath
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
$_nugetSource = "https://api.nuget.org/v3/index.json"
$_outputPath = Join-Path $_rootDirPath "publish"
$_testNugetSource = Join-Path $_rootDirPath "nuget"

function Main {
    Import-Module $_boltOnModulePath -Force
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    LogDebug "Branch: $branchName"
    BuildAndTest
    CleanUp
    PackAndPublish $branchName
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function CleanUp {
    Remove-Item $_outputPath -Recurse -ErrorAction Ignore
    Remove-Item $_testNugetSource -Recurse -ErrorAction Ignore
}

function PackAndPublish {
    param (
        [string]$branchName
    )

    if ($branchName) {
        $changedFiles = git diff "origin/$branchName...HEAD" --no-commit-id --name-only
        $changedFiles = $changedFiles | Where-Object { $_.ToString().StartsWith("src/", 1) } 
        $changedFiles
        if ($changedFiles.Length -gt 0) {
            $tempChangedProjects = $changedFiles | Select-Object `
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
            foreach ($key in $newVersions.keys) {
                $projectPath = Join-Path $_rootDirPath "src/$($key)/$($key).csproj"
                UpdateVersion $projectPath $newVersions[$key]
                dotnet pack $projectPath --configuration Release -o $_outputPath
                LogDebug "Packed package: $($key).$($newVersions[$key]).nupkg"
            }

            foreach ($key in $newVersions.keys) {
                if ($branchName -eq "master" -and $nugetApiKey) {
                    dotnet nuget push "$_outputPath/$($key).$($newVersions[$key]).nupkg" -k $nugetApiKey -s $_nugetSource
                    LogInfo "Published package: $($key).$($newVersions[$key]).nupkg"
                }
                else {
                    # this block is useful for testing
                    if (-Not(Test-Path $_testNugetSource)) {
                        New-Item -ItemType Directory -Force -Path $_testNugetSource
                    }
                    dotnet nuget push "$_outputPath/$($key).$($newVersions[$key]).nupkg" -s $_testNugetSource
                    LogInfo "Published package: $($key).$($newVersions[$key]).nupkg"
                }
            }
        }
    } 
}

function BuildAndTest {
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"
    dotnet build --configuration Release
    LogDebug "Built"
    dotnet test --configuration Release
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

Main