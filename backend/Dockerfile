FROM mcr.microsoft.com/dotnet/sdk:10.0.103 AS build
WORKDIR /app

COPY *.slnx .
COPY src/ src/
COPY tests/ tests/

RUN dotnet restore

RUN dotnet publish src/PetZone.API/PetZone.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "PetZone.API.dll"]
