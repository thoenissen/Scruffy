#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Scruffy.ServiceHosts.Discord/Scruffy.ServiceHosts.Discord.csproj"
WORKDIR "/src/Scruffy.ServiceHosts.Discord"
RUN dotnet build "Scruffy.ServiceHosts.Discord.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Scruffy.ServiceHosts.Discord.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Scruffy.ServiceHosts.Discord.dll"]