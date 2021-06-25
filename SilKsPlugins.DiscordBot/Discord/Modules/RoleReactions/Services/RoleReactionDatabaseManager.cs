using SilKsPlugins.DiscordBot.Databases.RoleReactions;
using SilKsPlugins.DiscordBot.Databases.RoleReactions.Models;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Modules.RoleReactions.Services
{
    public class RoleReactionDatabaseManager : IRoleReactionDatabaseManager
    {
        private readonly RoleReactionsDbContext _dbContext;

        public RoleReactionDatabaseManager(RoleReactionsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AddRoleToMessage(ulong guildId, ulong channelId, ulong messageId, ulong roleId)
        {
            if (await _dbContext.RoleMessages.FindAsync(guildId, channelId, messageId) != null)
            {
                return false;
            }

            var roleMessage = new RoleMessage
            {
                GuildId = guildId,
                ChannelId = channelId,
                MessageId = messageId,
                RoleId = roleId
            };

            await _dbContext.RoleMessages.AddAsync(roleMessage);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveRoleFromMessage(ulong guildId, ulong channelId, ulong messageId)
        {
            var roleMessage = await _dbContext.RoleMessages.FindAsync(guildId, channelId, messageId);

            if (roleMessage == null)
            {
                return false;
            }

            _dbContext.RoleMessages.Remove(roleMessage);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<ulong?> GetRoleForMessage(ulong guildId, ulong channelId, ulong messageId)
        {
            var roleMessage = await _dbContext.RoleMessages.FindAsync(guildId, channelId, messageId);

            return roleMessage?.RoleId;
        }
    }
}
