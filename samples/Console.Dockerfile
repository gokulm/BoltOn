# build
FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /app
COPY . .
RUN dotnet restore ./BoltOn.Samples.Console/BoltOn.Samples.Console.csproj
RUN dotnet publish -c Release ./BoltOn.Samples.Console/BoltOn.Samples.Console.csproj -o /publish

# runtime
FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app    
COPY --from=build /publish .

ENTRYPOINT ["dotnet", "BoltOn.Samples.Console.dll"]

# docker build -f Console.Dockerfile -t gokulm/bolton:bolton.samples.console .
# docker run --rm -it gokulm/bolton:bolton.samples.console
# if not the previous stop works
# docker run --rm --net=host  -it gokulm/bolton:bolton.samples.console