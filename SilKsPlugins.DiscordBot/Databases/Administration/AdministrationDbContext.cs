using Microsoft.EntityFrameworkCore;
using SilKsPlugins.DiscordBot.Databases.Administration.Models;
using SilKsPlugins.DiscordBot.MySql;
using System;

namespace SilKsPlugins.DiscordBot.Databases.Administration
{
    public class AdministrationDbContext : MySqlDbContext
    {
        public AdministrationDbContext(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override string GetConnectionStringName() => "Administration";

        public DbSet<LogChannel> LogChannels => Set<LogChannel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LogChannel>(entity =>
            {
                entity.HasKey(x => new {x.GuildId, x.ChannelId});

                entity.Property(x => x.GuildId)
                    .ValueGeneratedNever();

                entity.Property(x => x.ChannelId)
                    .ValueGeneratedNever();
            });

        }
    }
}
