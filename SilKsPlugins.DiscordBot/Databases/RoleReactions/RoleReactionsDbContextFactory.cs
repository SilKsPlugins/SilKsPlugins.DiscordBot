using JetBrains.Annotations;
using SilKsPlugins.DiscordBot.MySql;

namespace SilKsPlugins.DiscordBot.Databases.RoleReactions
{
    [UsedImplicitly]
    public class RoleReactionsDbContextFactory : MySqlDbContextFactory<RoleReactionsDbContext>
    {
    }
}
