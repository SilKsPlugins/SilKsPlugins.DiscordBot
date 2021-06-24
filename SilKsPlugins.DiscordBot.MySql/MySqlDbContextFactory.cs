using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SilKsPlugins.DiscordBot.MySql
{
    public class MySqlDbContextFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext> where TDbContext : MySqlDbContext
    {
        public TDbContext CreateDbContext(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddYamlFile("config.yaml", optional: false)
                .Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddEntityFrameworkMySql();
            serviceCollection.AddSingleton(config);
            serviceCollection.AddSingleton<IConfiguration>(config);
            serviceCollection.AddEntityFrameworkMySql();
            serviceCollection.AddDbContext<TDbContext>(ServiceLifetime.Transient, ServiceLifetime.Transient);
            
            var serviceProvider = serviceCollection.BuildServiceProvider();

            return serviceProvider.GetRequiredService<TDbContext>();
        }
    }
}
