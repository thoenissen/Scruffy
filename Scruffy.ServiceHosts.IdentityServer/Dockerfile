#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Scruffy.ServiceHosts.IdentityServer/Scruffy.ServiceHosts.IdentityServer.csproj", "Scruffy.ServiceHosts.IdentityServer/"]
COPY ["Scruffy.Data/Scruffy.Data.csproj", "Scruffy.Data/"]
RUN dotnet restore "Scruffy.ServiceHosts.IdentityServer/Scruffy.ServiceHosts.IdentityServer.csproj"
COPY . .
WORKDIR "/src/Scruffy.ServiceHosts.IdentityServer"
RUN dotnet build "Scruffy.ServiceHosts.IdentityServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Scruffy.ServiceHosts.IdentityServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Scruffy.ServiceHosts.IdentityServer.dll"]