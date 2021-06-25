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

            modelBuilder.Entity<LogChannel>()
                .Property(x => x.ChannelId)
                .ValueGeneratedNever();
        }
    }
}
