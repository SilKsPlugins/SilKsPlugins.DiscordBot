FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY "SilKsPlugins.DiscordBot/*.csproj" "SilKsPlugins.DiscordBot/"
RUN dotnet restore SilKsPlugins.DiscordBot

COPY . .
RUN dotnet publish SilKsPlugins.DiscordBot -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /data
COPY --from=build /app/publish /app
ENTRYPOINT ["dotnet", "/app/SilKsPlugins.DiscordBot.dll"]