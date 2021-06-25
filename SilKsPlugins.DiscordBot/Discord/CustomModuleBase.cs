using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SilKsPlugins.DiscordBot.Discord.Commands;
using System;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord
{
    public abstract class CustomModuleBase<T> : ModuleBase<T> where T : class, ICommandContext
    {
        private readonly CommandConfigAccessor _configuration;

        protected CustomModuleBase(IServiceProvider serviceProvider)
        {
            _configuration = serviceProvider.GetRequiredService<CommandConfigAccessor>();
        }

        protected virtual async Task<IUserMessage> ReplyAndDeleteAsync(
            string? message = null,
            bool isTTS = false,
            Embed? embed = null,
            RequestOptions? options = null,
            AllowedMentions? allowedMentions = null,
            MessageReference? messageReference = null,
            int? delay = null)
        {
            var reply = await ReplyAsync(message, isTTS, embed, options, allowedMentions, messageReference);

            delay ??= _configuration.DeleteStandardReplyDelay;

            await Task.Delay(delay.Value);

            await reply.DeleteAsync();

            return reply;
        }
    }
}
