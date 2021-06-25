using Discord.WebSocket;
using SilKsPlugins.DiscordBot.Components;
using SilKsPlugins.DiscordBot.Databases.Administration;
using SilKsPlugins.DiscordBot.Logging.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Logging
{
    public class LoggingComponent : IComponent
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly AdministrationDbContext _dbContext;
        private readonly DiscordSink _discordSink;
        private readonly IDiscordChannelLogConfigurer _discordLogConfigurer;
        private readonly IServiceProvider _serviceProvider;

        public LoggingComponent(DiscordSocketClient discordClient,
            AdministrationDbContext dbContext,
            DiscordSink discordSink,
            IDiscordChannelLogConfigurer discordLogConfigurer,
            IServiceProvider serviceProvider)
        {
            _discordClient = discordClient;
            _discordSink = discordSink;
            _discordLogConfigurer = discordLogConfigurer;
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _discordClient.Connected += OnDiscordClientConnected;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _discordClient.Connected -= OnDiscordClientConnected;

            return Task.CompletedTask;
        }

        private async Task OnDiscordClientConnected()
        {
            _discordSink.Setup(_serviceProvider);

            await foreach (var channel in _dbContext.LogChannels)
            {
                _discordLogConfigurer.AddChannel(channel.ChannelId);
            }
        }
    }
}
