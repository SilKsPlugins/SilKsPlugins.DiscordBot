using Microsoft.EntityFrameworkCore;
using SilKsPlugins.DiscordBot.Databases.Plugins.Models;
using SilKsPlugins.DiscordBot.MySql;
using System;

namespace SilKsPlugins.DiscordBot.Databases.Plugins
{
    public class PluginsDbContext : MySqlDbContext
    {
        public PluginsDbContext(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public DbSet<PluginInfo> Plugins => Set<PluginInfo>();

        public DbSet<CategoryInfo> Categories => Set<CategoryInfo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PluginInfo>(builder =>
            {
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Id)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<CategoryInfo>(builder =>
            {
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Id)
                    .ValueGeneratedNever();
            });
        }
    }
}
