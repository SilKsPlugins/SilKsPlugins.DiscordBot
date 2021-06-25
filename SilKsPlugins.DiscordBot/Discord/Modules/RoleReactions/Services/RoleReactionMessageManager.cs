using Discord;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Modules.RoleReactions.Services
{
    public class RoleReactionMessageManager : IRoleReactionMessageManager
    {
        private readonly IConfiguration _configuration;

        private IEmote[] GetReactions()
        {
            return new IEmote[]
            {
                new Emoji(_configuration["RoleReactions:Subscribe"]),
                new Emoji(_configuration["RoleReactions:Unsubscribe"])
            };
        }

        public RoleReactionMessageManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task AddReactionToMessage(IMessage message)
        {
            var emotes = GetReactions();

            await message.RemoveAllReactionsAsync();

            foreach (var emote in emotes)
            {
                await message.AddReactionAsync(emote);
            }
        }

        public async Task RemoveReactionFromMessage(IMessage message)
        {
            var emotes = GetReactions();

            foreach (var emote in emotes)
            {
                await message.RemoveAllReactionsForEmoteAsync(emote);
            }
        }
    }
}
