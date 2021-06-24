using Discord;
using Discord.Commands;
using JetBrains.Annotations;
using SilKsPlugins.DiscordBot.Discord.Preconditions;
using SilKsPlugins.DiscordBot.Logging.Configuration;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Administration
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class LoggingModule : ModuleBase<SocketCommandContext>
    {
        private readonly IDiscordChannelLogConfigurer _discordLogConfigurer;

        public LoggingModule(IDiscordChannelLogConfigurer discordLogConfigurer)
        {
            _discordLogConfigurer = discordLogConfigurer;
        }

        [Command("startlogging")]
        [Summary("Setup this channel to receive bot logs.")]
        [RequireBotAdmin]
        public async Task StartLoggingAsync()
        {
            var channelId = Context.Channel.Id;

            if (_discordLogConfigurer.AddChannel(channelId))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("This channel has been setup for logging.")
                    .WithColor(Color.Green)
                    .Build());
            }
            else
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("This channel is already setup for logging.")
                    .WithColor(Color.Red)
                    .Build());
            }
        }

        [Command("stoplogging")]
        [Summary("Remove this channel from the bot logging list.")]
        [RequireBotAdmin]
        public async Task StopLoggingAsync()
        {
            var channelId = Context.Channel.Id;

            if (_discordLogConfigurer.RemoveChannel(channelId))
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("This channel has been removed from logging.")
                    .WithColor(Color.Green)
                    .Build());
            }
            else
            {
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithTitle("This channel is not setup for logging.")
                    .WithColor(Color.Red)
                    .Build());
            }
        }
    }
}
