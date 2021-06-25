using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.RoleReactions.Services
{
    public interface IRoleReactionDatabaseManager
    {
        Task<ulong?> GetRoleForMessage(ulong guildId, ulong channelId, ulong messageId);

        Task<bool> AddRoleToMessage(ulong guildId, ulong channelId, ulong messageId, ulong roleId);

        Task<bool> RemoveRoleFromMessage(ulong guildId, ulong channelId, ulong messageId);
    }
}
