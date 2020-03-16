$_scriptDirPath = $PSScriptRoot
$ErrorActionPreference = 'stop'

function Main {
    try {
        $parentDirPath = Split-Path -parent $_scriptDirPath
        $buildDirPath = Join-Path $parentDirPath "build"
        $_boltOnModulePath = Join-Path $buildDirPath "bolton.psm1"

        Import-Module $_boltOnModulePath -Force
        LogBeginFunction "$($MyInvocation.MyCommand.Name)"

        dotnet tool install dnt --global --add-source=https://api.nuget.org/v3/index.json
        dnt switch-to-projects switcher.json

        docker-compose down 
        docker-compose -f docker-compose-local.yml up -d
        Start-Sleep -s 5

        RunProject "BoltOn.Samples.Console"
        RunProject "BoltOn.Samples.WebApi"
        
        LogEndFunction "$($MyInvocation.MyCommand.Name)"
    }
    catch {
        LogError $_.Exception.Message
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
    }
}

function RunProject()
{
    param(
        [parameter(Mandatory)]$projectName
    )

    LogDebug "Starting $projectName..."
    $projectDirPath = Join-Path $_scriptDirPath $projectName 
    $projectPath = Join-Path $projectDirPath "$projectName.csproj"
    $projectPath
    Start-Process -FilePath 'dotnet' -ArgumentList "run --project $projectPath"
    # dotnet run --project $projectPath
    LogDebug "Started $projectName"
}

# todo: need to find a better way
function IsDockerContainerRunning()
{
    param(
        [parameter(Mandatory)]$containerName
    )

    try {
        $result = docker inspect -f '{{.State.Running}}' $containerName
        return $result
    }
    catch {
        return $false
    }
}


Main

