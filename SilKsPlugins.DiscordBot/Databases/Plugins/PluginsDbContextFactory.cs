using JetBrains.Annotations;
using SilKsPlugins.DiscordBot.MySql;

namespace SilKsPlugins.DiscordBot.Databases.Plugins
{
    [UsedImplicitly]
    public class PluginsDbContextFactory : MySqlDbContextFactory<PluginsDbContext>
    {
    }
}
