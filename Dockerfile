FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

RUN --mount=type=secret,id=github_token \
  "dotnet nuget add source --username SilKsPlugins --password `cat /run/secrets/github_token` --store-password-in-clear-text --name github \"https://nuget.pkg.github.com/SilKsPlugins/index.json\""

COPY "SilKsPlugins.DiscordBot/*.csproj" "SilKsPlugins.DiscordBot/"
RUN dotnet restore SilKsPlugins.DiscordBot

COPY . .
RUN dotnet publish SilKsPlugins.DiscordBot -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /data
COPY --from=build /app/publish /app
ENTRYPOINT ["dotnet", "/app/SilKsPlugins.DiscordBot.dll"]