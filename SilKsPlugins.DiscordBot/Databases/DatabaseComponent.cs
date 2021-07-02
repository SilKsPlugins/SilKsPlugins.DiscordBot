using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SilKsPlugins.DiscordBot.Components;
using SilKsPlugins.DiscordBot.Databases.Administration;
using SilKsPlugins.DiscordBot.Databases.Plugins;
using SilKsPlugins.DiscordBot.Databases.RoleReactions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Databases
{
    [UsedImplicitly]
    public class DatabaseComponent : IComponent
    {
        private readonly ICollection<DbContext> _dbContexts;

        public DatabaseComponent(AdministrationDbContext administrationDbContext,
            PluginsDbContext pluginsDbContext,
            RoleReactionsDbContext roleReactionsDbContext)
        {
            _dbContexts = new DbContext[]
            {
                administrationDbContext,
                roleReactionsDbContext,
                pluginsDbContext,
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var dbContext in _dbContexts)
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
