using JetBrains.Annotations;
using SilKsPlugins.DiscordBot.MySql;

namespace SilKsPlugins.DiscordBot.Databases.Administration
{
    [UsedImplicitly]
    public class AdministrationDbContextFactory : MySqlDbContextFactory<AdministrationDbContext>
    {
    }
}
