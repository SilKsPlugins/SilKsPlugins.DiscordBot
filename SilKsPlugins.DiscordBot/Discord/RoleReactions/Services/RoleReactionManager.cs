using SilKsPlugins.DiscordBot.Databases.RoleReactions;
using SilKsPlugins.DiscordBot.Databases.RoleReactions.Models;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.RoleReactions.Services
{
    public class RoleReactionManager : IRoleReactionManager
    {
        private readonly RoleReactionsDbContext _dbContext;

        public RoleReactionManager(RoleReactionsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AddRoleToMessage(ulong channelId, ulong messageId, ulong roleId)
        {
            if (await _dbContext.RoleMessages.FindAsync(channelId, messageId) != null)
            {
                return false;
            }

            var roleMessage = new RoleMessage
            {
                ChannelId = channelId,
                MessageId = messageId,
                RoleId = roleId
            };

            await _dbContext.RoleMessages.AddAsync(roleMessage);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveRoleFromMessage(ulong channelId, ulong messageId)
        {
            var roleMessage = await _dbContext.RoleMessages.FindAsync(channelId, messageId);

            if (roleMessage == null)
            {
                return false;
            }

            _dbContext.RoleMessages.Remove(roleMessage);

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<ulong?> GetRoleForMessage(ulong channelId, ulong messageId)
        {
            var roleMessage = await _dbContext.RoleMessages.FindAsync(channelId, messageId);

            return roleMessage?.RoleId;
        }
    }
}
