using Autofac;
using JetBrains.Annotations;
using SilKsPlugins.DiscordBot.Discord.Modules.RoleReactions.Services;
using SilKsPlugins.DiscordBot.IoC;

namespace SilKsPlugins.DiscordBot.Discord.Modules.RoleReactions
{
    [UsedImplicitly]
    public class RoleReactionsContainerConfigurator : IContainerConfigurator
    {
        public void ConfigureContainer(ContainerConfiguratorContext context)
        {
            // Role reactions
            context.ContainerBuilder.RegisterType<RoleReactionDatabaseManager>()
                .AsSelf()
                .As<IRoleReactionDatabaseManager>()
                .InstancePerDependency();

            context.ContainerBuilder.RegisterType<RoleReactionMessageManager>()
                .AsSelf()
                .As<IRoleReactionMessageManager>()
                .InstancePerDependency();
        }
    }
}
