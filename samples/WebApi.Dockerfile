# build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /app
COPY . .
RUN dotnet restore ./BoltOn.Samples.WebApi/BoltOn.Samples.WebApi.csproj
RUN dotnet publish -c Release ./BoltOn.Samples.WebApi/BoltOn.Samples.WebApi.csproj -o /publish

# runtime
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2.8
WORKDIR /app    
COPY --from=build /publish .

ENTRYPOINT ["dotnet", "BoltOn.Samples.WebApi.dll"]

# docker build -f Dockerfile.WebApi -t gokulm/bolton:bolton.samples.webapi .
# docker run --rm -it -p 5000:80 gokulm/bolton:bolton.samples.webapi