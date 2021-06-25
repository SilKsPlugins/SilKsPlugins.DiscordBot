using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SilKsPlugins.DiscordBot.Components;
using SilKsPlugins.DiscordBot.Discord.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord
{
    public class DiscordBotService : IHostedService
    {
        private readonly Runtime _runtime;
        private readonly ILogger<DiscordBotService> _logger;
        private readonly IConfiguration _configuration;
        private readonly CommandHandler _commandHandler;
        private readonly DiscordSocketClient _client;
        private readonly ComponentManager _componentManager;

        public DiscordBotService(Runtime runtime,
            ILogger<DiscordBotService> logger,
            IConfiguration configuration,
            CommandHandler commandHandler,
            DiscordSocketClient client,
            ComponentManager componentManager)
        {
            _runtime = runtime;
            _logger = logger;
            _configuration = configuration;
            _commandHandler = commandHandler;
            _client = client;
            _componentManager = componentManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var token = _configuration["DiscordToken"];

            if (string.IsNullOrWhiteSpace(token) || token == "CHANGEME")
            {
                _logger.LogCritical("A token must be specified in the config file.");

                // We must close the application by directly stopping the runtime
                // as if we call Environment.Exit, the method won't return until the
                // application has exited. The application however won't exit until this
                // method has returned resulting the application locking.

                // ReSharper disable once MethodSupportsCancellation
                await _runtime.Host.StopAsync(cancellationToken);

                return;
            }

            await _componentManager.StartAsync(cancellationToken);

            _client.Log += OnLog;

            await _commandHandler.InstallCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.StopAsync();

            _client.Log -= OnLog;

            await _componentManager.StopAsync(cancellationToken);
        }

        private Task OnLog(LogMessage log)
        {
            var logLevel = log.Severity switch
            {
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Debug,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Critical => LogLevel.Critical,
                _ => LogLevel.None
            };

            if (log.Exception == null)
            {
                _logger.Log(logLevel, log.Message);
            }
            else
            {
                _logger.Log(logLevel, log.Exception, log.Message);
            }

            return Task.CompletedTask;
        }
    }
}
