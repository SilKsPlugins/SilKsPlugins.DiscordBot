using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SilKsPlugins.DiscordBot.IoC
{
    public class ServiceConfiguratorContext
    {
        public IServiceCollection ServiceCollection { get; }

        public IConfiguration Configuration { get; }

        public ServiceConfiguratorContext(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            ServiceCollection = serviceCollection;
            Configuration = configuration;
        }
    }
}