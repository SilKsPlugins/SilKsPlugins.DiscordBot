using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SilKsPlugins.DiscordBot.Databases.Administration;
using SilKsPlugins.DiscordBot.Databases.RoleReactions;
using SilKsPlugins.DiscordBot.IoC;

namespace SilKsPlugins.DiscordBot.Databases
{
    [UsedImplicitly]
    public class DatabaseServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(ServiceConfiguratorContext context)
        {
            context.ServiceCollection.AddEntityFrameworkMySql()
                .AddDbContext<AdministrationDbContext>(ServiceLifetime.Transient)
                .AddDbContext<RoleReactionsDbContext>(ServiceLifetime.Transient);
        }
    }
}
