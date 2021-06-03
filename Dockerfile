FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SilKsPlugins.DiscordBot/SilKsPlugins.DiscordBot.csproj", "SilKsPlugins.DiscordBot/"]
RUN dotnet restore "SilKsPlugins.DiscordBot/SilKsPlugins.DiscordBot.csproj"
COPY . .
WORKDIR "/src/SilKsPlugins.DiscordBot"
RUN dotnet build "SilKsPlugins.DiscordBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SilKsPlugins.DiscordBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SilKsPlugins.DiscordBot.dll"]