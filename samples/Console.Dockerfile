# build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /app
COPY . .
RUN dotnet restore ./BoltOn.Samples.Console/BoltOn.Samples.Console.csproj
RUN dotnet publish -c Release ./BoltOn.Samples.Console/BoltOn.Samples.Console.csproj -o /publish

# runtime
FROM mcr.microsoft.com/dotnet/core/runtime:2.2
WORKDIR /app    
COPY --from=build /publish .

ENTRYPOINT ["dotnet", "BoltOn.Samples.Console.dll"]

# docker build -f Dockerfile.Console -t gokulm/bolton:bolton.samples.console .
# docker run --rm -it gokulm/bolton:bolton.samples.console