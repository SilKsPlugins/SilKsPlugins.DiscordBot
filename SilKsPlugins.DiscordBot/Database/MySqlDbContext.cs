using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SilKsPlugins.DiscordBot.Database
{
    public class MySqlDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        protected MySqlDbContext(IServiceProvider serviceProvider)
        {
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(_configuration["Database:ConnectionStrings:Default"],
                MariaDbServerVersion.LatestSupportedServerVersion);
        }
    }
}
