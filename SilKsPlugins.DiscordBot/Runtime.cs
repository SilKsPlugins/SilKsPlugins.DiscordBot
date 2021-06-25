using Autofac;
using Autofac.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SilKsPlugins.DiscordBot.Commands;
using SilKsPlugins.DiscordBot.Databases.Administration;
using SilKsPlugins.DiscordBot.Databases.RoleReactions;
using SilKsPlugins.DiscordBot.Discord;
using SilKsPlugins.DiscordBot.Discord.RoleReactions.Services;
using SilKsPlugins.DiscordBot.Logging;
using SilKsPlugins.DiscordBot.Logging.Configuration;
using System;
using System.IO;
using System.Resources;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot
{
    public class Runtime
    {
        public IHost Host { get; private set; } = null!;

        public IConfiguration Configuration { get; private set; } = null!;

        public string WorkingDirectory { get; private set; } = null!;

        private readonly DiscordSink _discordSink = new();

        public async Task InitAsync()
        {
            var hostBuilder = new HostBuilder();

            WorkingDirectory = Environment.CurrentDirectory;

            SetupSerilog();
            
            hostBuilder
                .UseContentRoot(WorkingDirectory)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureAppConfiguration(ConfigureConfiguration)
                .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
                .ConfigureServices(SetupServices)
                .UseSerilog();

            using (Host = hostBuilder.Build())
            {
                await Host.RunAsync();
            }
        }

        private void ExportResource(string resource)
        {
            var resourcePath = Path.Combine(WorkingDirectory, resource);

            if (File.Exists(resourcePath)) return;

            var assembly = GetType().Assembly;

            using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{resource}");
            using var reader = new StreamReader(stream ?? throw new MissingManifestResourceException("Missing embedded resource"));

            var contents = reader.ReadToEnd();

            File.WriteAllText(resourcePath, contents);
        }

        private void SetupSerilog()
        {
            const string configPath = "logging.yaml";

            ExportResource(configPath);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(WorkingDirectory)
                .AddYamlFile(configPath)
                .Build();

            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.Sink(_discordSink)
                .ReadFrom.Configuration(configuration);

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        private void ConfigureConfiguration(IConfigurationBuilder builder)
        {
            const string configPath = "config.yaml";

            ExportResource(configPath);

            builder.SetBasePath(WorkingDirectory)
                .AddYamlFile(configPath, optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }

        private void ConfigureContainer(ContainerBuilder container)
        {
            container.RegisterType<RoleReactionDatabaseManager>()
                .AsSelf()
                .As<IRoleReactionDatabaseManager>()
                .InstancePerDependency();
        }

        private void SetupServices(IServiceCollection services)
        {
            // Runtime
            services.AddSingleton(this);

            // Logging
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true))
                .AddSingleton<IDiscordChannelLogConfigurer>(new DiscordChannelLogConfigurer())
                .AddSingleton(_discordSink);

            // Databases
            services.AddEntityFrameworkMySql()
                .AddDbContext<AdministrationDbContext>(ServiceLifetime.Transient)
                .AddDbContext<RoleReactionsDbContext>(ServiceLifetime.Transient);

            // Discord-related
            services
                .AddSingleton<CommandHandler>()
                .AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig {AlwaysDownloadUsers = true}))
                .AddTransient(_ => new CommandService(new CommandServiceConfig {DefaultRunMode = RunMode.Async}))
                .AddHostedService<DiscordBotService>();
        }
    }
}
