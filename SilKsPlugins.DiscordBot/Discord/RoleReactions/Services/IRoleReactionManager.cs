using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.RoleReactions.Services
{
    public interface IRoleReactionManager
    {
        Task<ulong?> GetRoleForMessage(ulong channelId, ulong messageId);

        Task<bool> AddRoleToMessage(ulong channelId, ulong messageId, ulong roleId);

        Task<bool> RemoveRoleFromMessage(ulong channelId, ulong messageId);
    }
}
