function Main
{
    BuildAndTest
}

function BuildAndTest
{
    dotnet build --configuration Release
    dotnet test --configuration Release
}

Main