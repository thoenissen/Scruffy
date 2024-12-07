# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Scruffy.WebApp/Scruffy.WebApp.csproj", "Scruffy.WebApp/"]
COPY ["Scruffy.Data/Scruffy.Data.csproj", "Scruffy.Data/"]
RUN dotnet restore "./Scruffy.WebApp/Scruffy.WebApp.csproj"
COPY . .
WORKDIR "/src/Scruffy.WebApp"
RUN dotnet build "./Scruffy.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Scruffy.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Scruffy.WebApp.dll"]