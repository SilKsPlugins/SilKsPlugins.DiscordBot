using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SilKsPlugins.DiscordBot.Discord.Modules.Plugins.Services;
using SilKsPlugins.DiscordBot.IoC;

namespace SilKsPlugins.DiscordBot.Discord.Modules.Plugins
{
    [UsedImplicitly]
    public class PluginsServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(ServiceConfiguratorContext context)
        {
            context.ServiceCollection.AddSingleton<MerchantIdAccessor>();
        }
    }
}
