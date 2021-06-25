using Discord;
using Discord.Commands;
using JetBrains.Annotations;
using SilKsPlugins.DiscordBot.Commands;
using SilKsPlugins.DiscordBot.Databases.Administration;
using SilKsPlugins.DiscordBot.Databases.Administration.Models;
using SilKsPlugins.DiscordBot.Discord.Preconditions;
using SilKsPlugins.DiscordBot.Helpers;
using SilKsPlugins.DiscordBot.Logging.Configuration;
using System;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Modules.Administration
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class LoggingModule : CustomModuleBase<SocketCommandContext>
    {
        private readonly IDiscordChannelLogConfigurer _discordLogConfigurer;
        private readonly AdministrationDbContext _dbContext;

        public LoggingModule(IDiscordChannelLogConfigurer discordLogConfigurer,
            AdministrationDbContext dbContext,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _discordLogConfigurer = discordLogConfigurer;
            _dbContext = dbContext;
        }

        [Command("startlogging")]
        [Summary("Setup this channel to receive bot logs.")]
        [RequireBotAdmin]
        public async Task StartLoggingAsync()
        {
            var channelId = Context.Channel.Id;

            var logChannel = await _dbContext.LogChannels.FindAsync(channelId);

            if (logChannel == null)
            {
                await _dbContext.LogChannels.AddAsync(new LogChannel
                {
                    ChannelId = channelId
                });

                await _dbContext.SaveChangesAsync();
            }

            if (_discordLogConfigurer.AddChannel(channelId))
            {
                await ReplyAndDeleteAsync(embed: EmbedHelper.SimpleEmbed("This channel has been setup for logging.",
                    Color.Green));
            }
            else
            {
                throw new UserFriendlyException("This channel is already setup for logging.");
            }
        }

        [Command("stoplogging")]
        [Summary("Remove this channel from the bot logging list.")]
        [RequireBotAdmin]
        public async Task StopLoggingAsync()
        {
            var channelId = Context.Channel.Id;

            var logChannel = await _dbContext.LogChannels.FindAsync(channelId);

            if (logChannel != null)
            {
                _dbContext.LogChannels.Remove(logChannel);

                await _dbContext.SaveChangesAsync();
            }

            if (_discordLogConfigurer.RemoveChannel(channelId))
            {
                await ReplyAndDeleteAsync(embed: EmbedHelper.SimpleEmbed("This channel has been removed from logging.",
                    Color.Green));
            }
            else
            {
                throw new UserFriendlyException("This channel is not setup for logging.");
            }
        }
    }
}
