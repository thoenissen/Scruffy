#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Scruffy.ServiceHosts.Discord/Scruffy.ServiceHosts.Discord.csproj", "Scruffy.ServiceHosts.Discord/"]
COPY ["Scruffy.Data/Scruffy.Data.csproj", "Scruffy.Data/"]
COPY ["Scruffy.Services/Scruffy.Services.csproj", "Scruffy.Services/"]
COPY ["Scruffy.Commands/Scruffy.Commands.csproj", "Scruffy.Commands/"]
RUN dotnet restore "Scruffy.ServiceHosts.Discord/Scruffy.ServiceHosts.Discord.csproj"
COPY . .
WORKDIR "/src/Scruffy.ServiceHosts.Discord"
RUN dotnet build "Scruffy.ServiceHosts.Discord.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Scruffy.ServiceHosts.Discord.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Scruffy.ServiceHosts.Discord.dll"]