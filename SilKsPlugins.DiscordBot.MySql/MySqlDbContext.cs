using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SilKsPlugins.DiscordBot.MySql
{
    public abstract class MySqlDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        protected MySqlDbContext(IServiceProvider serviceProvider)
        {
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
        }

        protected virtual string GetConnectionStringName() => "Default";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringName = GetConnectionStringName();
            var connectionString = _configuration[$"Database:ConnectionStrings:{connectionStringName}"];

            optionsBuilder.UseMySql(connectionString, MariaDbServerVersion.LatestSupportedServerVersion);
        }
    }
}
