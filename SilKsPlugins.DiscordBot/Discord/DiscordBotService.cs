using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using SilKsPlugins.DiscordBot.Commands;
using SilKsPlugins.DiscordBot.Logging;
using System;
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
        private readonly IServiceProvider _serviceProvider;

        public DiscordBotService(
            Runtime runtime,
            ILogger<DiscordBotService> logger,
            IConfiguration configuration,
            CommandHandler commandHandler,
            DiscordSocketClient client,
            IServiceProvider serviceProvider)
        {
            _runtime = runtime;
            _logger = logger;
            _configuration = configuration;
            _commandHandler = commandHandler;
            _client = client;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var token = _configuration["Token"];

            if (string.IsNullOrWhiteSpace(token) || token == "CHANGEME")
            {
                _logger.LogCritical("A token must be specified in the config file.");

                // We must close the application by directly stopping the runtime
                // as if we call Environment.Exit, the method won't return until the
                // application has exited. The application however won't exit until this
                // method has returned resulting the application locking.

                // ReSharper disable once MethodSupportsCancellation
                await _runtime.Host.StopAsync();

                return;
            }

            _client.Log += OnLog;

            await _commandHandler.InstallCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Sink((ILogEventSink)Log.Logger)
                .WriteTo.DiscordSink(_serviceProvider)
                .CreateLogger();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.StopAsync();

            _client.Log -= OnLog;
        }

        private Task OnLog(LogMessage arg)
        {
            _logger.LogInformation(arg.Message);

            return Task.CompletedTask;
        }
    }
}
