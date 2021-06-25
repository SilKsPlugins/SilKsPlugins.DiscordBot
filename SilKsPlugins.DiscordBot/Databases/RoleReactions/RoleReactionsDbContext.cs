using Microsoft.EntityFrameworkCore;
using SilKsPlugins.DiscordBot.Databases.RoleReactions.Models;
using SilKsPlugins.DiscordBot.MySql;
using System;

namespace SilKsPlugins.DiscordBot.Databases.RoleReactions
{
    public class RoleReactionsDbContext : MySqlDbContext
    {
        public RoleReactionsDbContext(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public DbSet<RoleMessage> RoleMessages => Set<RoleMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RoleMessage>(entity =>
            {
                entity.HasKey(x => new {x.GuildId, x.ChannelId, x.MessageId});

                entity.Property(x => x.GuildId)
                    .ValueGeneratedNever();

                entity.Property(x => x.ChannelId)
                    .ValueGeneratedNever();

                entity.Property(x => x.MessageId)
                    .ValueGeneratedNever();

                entity.Property(x => x.RoleId)
                    .ValueGeneratedNever();
            });
        }
    }
}
