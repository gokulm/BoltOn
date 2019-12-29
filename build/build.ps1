$_scriptDirPath = $PSScriptRoot
$_boltOnModulePath = Join-Path $_scriptDirPath "bolton.psm1"

function Main
{
    Import-Module $_boltOnModulePath -Force
    Write-BeginFunction "$($MyInvocation.MyCommand.Name)"
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