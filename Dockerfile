# build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /app
COPY /src/ ./src/
COPY /samples/ ./samples/
RUN dotnet restore ./samples/BoltOn.Samples.WebApi/BoltOn.Samples.WebApi.csproj
RUN dotnet publish -c Release ./samples/BoltOn.Samples.WebApi/BoltOn.Samples.WebApi.csproj -o /publish

# runtime
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /publish .

ENTRYPOINT ["dotnet", "BoltOn.Samples.WebApi.dll"]

# docker build -t gokulm/bolton:bolton.samples.webapi