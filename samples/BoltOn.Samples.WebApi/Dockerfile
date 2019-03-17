FROM mcr.microsoft.com/dotnet/core/aspnet:2.2

WORKDIR /app
COPY bin/Debug/netcoreapp2.2/publish .

ENTRYPOINT ["dotnet", "/app/BoltOn.Samples.WebApi.dll"]