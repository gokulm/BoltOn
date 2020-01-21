Param([string]$_branchName, [string]$_nugetApiKey)

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
        # uncomment Test after fixing all the integration tests
        # Test
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
    $changedFiles = git diff --name-only "HEAD^..HEAD" 
    # $changedFiles = git diff --name-only master...
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
        # $newVersions = GetProjectNewVersions "feat: test" $changedProjects
        $newVersions = GetProjectNewVersions $commits[0] $changedProjects
        $newVersions
            
        # nuget pack
        foreach ($key in $newVersions.keys) {
            $projectPath = Join-Path $_rootDirPath "src/$key/$key.csproj"
            $newVersion = $newVersions.$key
            UpdateVersion $projectPath $newVersion
            dotnet pack $projectPath --configuration Release -o $_outputPath
            LogDebug "Packed package: $key.$newVersion.nupkg"
        }

        # nuget publish and git tag
        foreach ($key in $newVersions.keys) {
            $newVersion = $newVersions.$key
            if ($_branchName -eq "master") {
                if (-Not($_nugetApiKey)) {
                    throw "NuGet API key not found"
                }
                dotnet nuget push "$_outputPath/$key.$newVersion.nupkg" -k $_nugetApiKey -s $_nugetSource
                LogInfo "Published package: $key.$newVersion.nupkg"
            }
            else {
                # this block is useful for testing
                if (-Not(Test-Path $_testNugetSource)) {
                    New-Item -ItemType Directory -Force -Path $_testNugetSource
                }
                dotnet nuget push "$_outputPath/$key.$newVersion.nupkg" -s $_testNugetSource
                LogInfo "Published package: $key.$newVersion.nupkg"
            }
        }

        # git tag
        if ($_branchName -eq "master") {
            foreach ($key in $newVersions.keys) {
                git tag "$key.$newVersion"
                LogDebug "Git tagged: $key.$newVersion"
            }

            git push origin --tags
            LogInfo "Pushed Git Tags"
        }
    } 
    LogEndFunction "$($MyInvocation.MyCommand.Name)"
}

Main