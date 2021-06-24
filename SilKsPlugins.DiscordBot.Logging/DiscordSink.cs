using Discord;
using Discord.WebSocket;
using Serilog.Core;
using Serilog.Events;
using SilKsPlugins.DiscordBot.Logging.Configuration;
using System;
using System.Diagnostics;

namespace SilKsPlugins.DiscordBot.Logging
{
    public class DiscordSink : ILogEventSink
    {
        private readonly IFormatProvider? _formatProvider;
        private readonly DiscordSocketClient _discordClient;
        private readonly IDiscordChannelLogConfigurer _discordChannels;

        public DiscordSink(DiscordSocketClient discordClient,
            IDiscordChannelLogConfigurer discordChannels)
        {
            _discordClient = discordClient;
            _discordChannels = discordChannels;
        }

        public DiscordSink(IFormatProvider formatProvider,
            DiscordSocketClient discordClient,
            IDiscordChannelLogConfigurer discordChannels) : this(discordClient, discordChannels)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            if (_discordClient.ConnectionState != ConnectionState.Connected)
            {
                return;
            }

            var message = logEvent.RenderMessage(_formatProvider);

            foreach (var channelId in _discordChannels.GetChannelIds())
            {
                try
                {
                    if (_discordClient.GetChannel(channelId) is not IMessageChannel channel)
                    {
                        Debug.WriteLine("Discord channel for logging does not exist: {0}", channelId);
                        continue;
                    }

                    channel.SendMessageAsync(message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception occurred when writing Discord log: {0}", ex);
                }
            }
        }
    }
}
