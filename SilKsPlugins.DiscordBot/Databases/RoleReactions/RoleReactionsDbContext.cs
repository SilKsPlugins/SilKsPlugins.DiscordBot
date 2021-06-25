using Microsoft.EntityFrameworkCore;
using SilKsPlugins.DiscordBot.Databases.RoleReactions.Models;
using SilKsPlugins.DiscordBot.MySql;
using System;

namespace SilKsPlugins.DiscordBot.Databases.RoleReactions
{
    public class RoleReactionsDbContext : MySqlDbContext
    {
        protected RoleReactionsDbContext(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RoleMessage>(entity =>
            {
                entity.HasKey(x => new {x.ChannelId, x.MessageId});

                entity.Property(x => x.ChannelId)
                    .ValueGeneratedNever();

                entity.Property(x => x.MessageId)
                    .ValueGeneratedNever();

                entity.Property(x => x.RoleId)
                    .ValueGeneratedNever();
            });
        }

        public DbSet<RoleMessage> RoleMessages => Set<RoleMessage>();
    }
}
