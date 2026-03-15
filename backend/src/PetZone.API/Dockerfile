FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/PetZone.API/PetZone.API.csproj", "src/PetZone.API/"]
COPY ["src/PetZone.Application/PetZone.Application.csproj", "src/PetZone.Application/"]
COPY ["src/PetZone.Domain/PetZone.Domain.csproj", "src/PetZone.Domain/"]
COPY ["src/PetZone.Infrastructure/PetZone.Infrastructure.csproj", "src/PetZone.Infrastructure/"]
RUN dotnet restore "src/PetZone.API/PetZone.API.csproj"
COPY . .
WORKDIR "/src/src/PetZone.API"
RUN dotnet build "./PetZone.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PetZone.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PetZone.API.dll"]
