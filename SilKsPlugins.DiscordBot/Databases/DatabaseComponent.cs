using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SilKsPlugins.DiscordBot.Components;
using SilKsPlugins.DiscordBot.Databases.Administration;
using SilKsPlugins.DiscordBot.Databases.RoleReactions;
using System.Threading;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Databases
{
    [UsedImplicitly]
    public class DatabaseComponent : IComponent
    {
        private readonly AdministrationDbContext _administrationDbContext;
        private readonly RoleReactionsDbContext _roleReactionsDbContext;

        public DatabaseComponent(AdministrationDbContext administrationDbContext,
            RoleReactionsDbContext roleReactionsDbContext)
        {
            _administrationDbContext = administrationDbContext;
            _roleReactionsDbContext = roleReactionsDbContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _administrationDbContext.Database.MigrateAsync(cancellationToken);
            await _roleReactionsDbContext.Database.MigrateAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
