using Microsoft.Extensions.DependencyInjection;
using SilKsPlugins.DiscordBot.IoC;

namespace SilKsPlugins.DiscordBot.Discord.Modules.Plugins.Services
{
    public class PluginsServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(ServiceConfiguratorContext context)
        {
            context.ServiceCollection.AddSingleton<MerchantIdAccessor>();
        }
    }
}
