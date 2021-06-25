using Discord.Commands;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SilKsPlugins.DiscordBot.IoC;

namespace SilKsPlugins.DiscordBot.Discord.Commands
{
    [UsedImplicitly]
    public class CommandsServiceConfigurator : IServiceConfigurator
    {
        public void ConfigureServices(ServiceConfiguratorContext context)
        {
            context.ServiceCollection.AddSingleton<CommandHandler>()
                .AddSingleton<CommandConfigAccessor>()
                .AddTransient(_ => new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async
                }));
        }
    }
}
