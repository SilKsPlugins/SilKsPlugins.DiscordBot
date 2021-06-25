using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SilKsPlugins.DiscordBot.IoC;

namespace SilKsPlugins.DiscordBot.Components
{
    [UsedImplicitly]
    public class ComponentServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(ServiceConfiguratorContext context)
        {
            context.ServiceCollection.AddSingleton<ComponentManager>();
        }
    }
}
