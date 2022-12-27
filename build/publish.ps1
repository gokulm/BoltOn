Param(
    [string]$_branchName, 
    [string]$_nugetApiKey, 
    [string]$GITHUB_ACTOR,
    [string]$GITHUB_TOKEN
)

$_scriptDirPath = $PSScriptRoot
$_rootDirPath = Split-Path $_scriptDirPath
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"
$_nugetSource = "https://api.nuget.org/v3/index.json"
$_outputPath = Join-Path $_rootDirPath "publish"
$_testNugetSource = Join-Path $_rootDirPath "nuget"
$ErrorActionPreference = 'stop'

function Main {
    try {
        Import-Module $_boltOnModulePath -Force
        LogBeginFunction "$($MyInvocation.MyCommand.Name)"
        LogDebug "Branch: $_branchName"
        Build
        Test
        CleanUp
        # this is invoked in develop branch only to test packaging and publishing
        # the packages are published only to local folder in develop branch
        NuGetPackAndPublish 
        LogEndFunction "$($MyInvocation.MyCommand.Name)"
    }
    catch {
        LogError $_.Exception.Message
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
    }
}

function CleanUp {
    Remove-Item $_outputPath -Recurse -ErrorAction Ignore
    Remove-Item $_testNugetSource -Recurse -ErrorAction Ignore
}

function NuGetPackAndPublish {
    LogBeginFunction "$($MyInvocation.MyCommand.Name)"

    $commits = git log -n 1 --pretty=%B
    # $commits = "feat(BoltOn, BoltOn.Data.EF, BoltOn.Data.CosmosDb, BoltOn.Bus.MassTransit): test"
    # $commits = "feat: test"
    $scope = GetConventionalCommitScope $commits
    $newVersions = @{ }

    if ($null -ne $scope -and $scope.ToString().Trim()) {
        LogDebug "Scope: $scope"
        $newVersions = GetProjectNewVersions $commits $null
    }
    else {
        LogDebug "Scope not found..."
        if ($_branchName -eq "master") {
            $changedFiles = git diff --name-only "HEAD^..HEAD" 
        }
        else {
            $changedFiles = git diff --name-only "origin/$_branchName...HEAD" 
        }
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
            $newVersions = GetProjectNewVersions $commits $changedProjects
        }
    }

    $newVersions
    if ($newVersions.keys.Length -eq 0) {
        LogDebug "No version changes"
        return
    }

    # if the core BoltOn exists, then pack and publish it first 
    # when projects with multiple level of dependencies are added, this should be changed
    if ($newVersions.BoltOn) {
        $version = $newVersions.BoltOn
        NugetPack "BoltOn" $version

        $tag = "BoltOn.$version"
        GitTag $tag

        NugetPublish "BoltOn" $version
        $newVersions = $newVersions | Where-Object { $_.key –ne "BoltOn" }
    }
            
    # nuget pack
    foreach ($key in $newVersions.keys) {
        $newVersion = $newVersions.$key
        NugetPack $key $newVersion 
    }

    # git tag
    foreach ($key in $newVersions.keys) {
        $newVersion = $newVersions.$key
        $tag = "$key.$newVersion"
        GitTag $tag
    }

    # nuget publish and git tag
    foreach ($key in $newVersions.keys) {
        $newVersion = $newVersions.$key
        NugetPublish $key $newVersion
    }

    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

function NugetPack {
    param (
        [string]$key,
        [string]$value
    )
    
    $projectPath = Join-Path $_rootDirPath "src/$key/$key.csproj"
    UpdateVersion $projectPath $value
    dotnet pack $projectPath --configuration Release -o $_outputPath
    LogDebug "Packed package: $key.$value.nupkg"
}

function NugetPublish {
    param (
        [string]$key,
        [string]$value
    )
    
    if ($_branchName -eq "master") {
        if (-Not($_nugetApiKey)) {
            throw "NuGet API key not found"
        }
        dotnet nuget push "$_outputPath/$key.$value.nupkg" -k $_nugetApiKey -s $_nugetSource
        LogInfo "Published package: $key.$value.nupkg"
    }
    else {
        # this block is useful for testing
        if (-Not(Test-Path $_testNugetSource)) {
            New-Item -ItemType Directory -Force -Path $_testNugetSource
        }
        dotnet nuget push "$_outputPath/$key.$value.nupkg" -s $_testNugetSource
        LogInfo "Published package: $key.$value.nupkg"
    }
}

function GitTag {
    param ([string]$tag)

    try {
        LogDebug "About to tag: $tag"
        if ($_branchName -eq "master") {
            git tag "$tag"
            LogDebug "Git tagged: $tag"
            if ($null -ne $GITHUB_ACTOR -and $null -ne $GITHUB_TOKEN) {
                git push "https://${GITHUB_ACTOR}:$GITHUB_TOKEN@github.com/gokulm/BoltOn.git" tag $tag
            }
            else {
                git push origin tag $tag
            }
            LogInfo "Pushed Git Tag $tag"
        }
    }
    catch {
        LogError $_.Exception.Message
    }
}

Main