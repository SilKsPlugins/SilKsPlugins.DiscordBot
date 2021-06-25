using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using SilKsPlugins.DiscordBot.Logging.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Logging
{
    public class DiscordSink : ILogEventSink
    {
        private readonly IFormatProvider? _formatProvider;
        private DiscordSocketClient? _discordClient;
        private IDiscordChannelLogConfigurer? _discordChannels;

        public DiscordSink()
        {
        }

        public DiscordSink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public void Setup(IServiceProvider serviceProvider)
        {
            _discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
            _discordChannels = serviceProvider.GetRequiredService<IDiscordChannelLogConfigurer>();
        }

        public void Emit(LogEvent logEvent)
        {
            if (_discordClient == null || _discordChannels == null)
            {
                return;
            }

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

                    var embed = new EmbedBuilder()
                        .AddField(logEvent.Level.ToString(), message)
                        .WithCurrentTimestamp()
                        .Build();

                    if (logEvent.Exception == null)
                    {
                        Task.Run(async () =>
                        {
                            await channel.SendMessageAsync(embed: embed);
                        });
                    }
                    else
                    {
                        var exceptionStr = logEvent.Exception.ToString();

                        var stream = new MemoryStream();
                        var writer = new StreamWriter(stream);
                        writer.Write(exceptionStr);
                        writer.Flush();
                        stream.Position = 0;

                        Task.Run(async () =>
                        {
                            await channel.SendMessageAsync(embed: embed);
                            await channel.SendFileAsync(stream, "Exception.txt");
                        });
                    }
                    
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception occurred when writing Discord log: {0}", ex);
                }
            }
        }
    }
}
