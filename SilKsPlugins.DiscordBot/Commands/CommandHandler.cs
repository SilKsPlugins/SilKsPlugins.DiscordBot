using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SilKsPlugins.DiscordBot.Helpers;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Commands
{
    public class CommandHandler : IDisposable
    {
        private readonly ILogger<CommandHandler> _logger;
        private readonly CommandConfigAccessor _configuration;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;

        public CommandService Commands { get; }

        public float DefaultDeleteReplyWait => 15000;

        public CommandHandler(
            ILogger<CommandHandler> logger,
            CommandConfigAccessor configuration,
            DiscordSocketClient client,
            CommandService commands,
            IServiceProvider services)
        {
            _logger = logger;
            _configuration = configuration;
            _client = client;
            _services = services;

            Commands = commands;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
        
        public void Dispose()
        {
            _client.MessageReceived -= HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            if (message is not SocketUserMessage userMessage) return;

            var argPos = 0;

            var prefix = _configuration.CommandPrefix;

            if (string.IsNullOrWhiteSpace(prefix)) return;

            if (!userMessage.HasStringPrefix(prefix, ref argPos) || message.Author.IsBot)
                return;
            
            var context = new SocketCommandContext(_client, userMessage);

            _logger.LogInformation($"Executing command '{userMessage.Content}' from user {userMessage.Author}.");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = await Commands.ExecuteAsync(context, argPos, _services);

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                _logger.LogDebug(
                    $"Successfully executed command {message.Content} in {stopwatch.ElapsedMilliseconds} ms");
            }
            else if (result is ExecuteResult
            {
                Error: CommandError.Exception,
                Exception: UserFriendlyException
            } execResult)
            {
                var reply = await message.Channel.SendMessageAsync(embed: EmbedHelper.SimpleEmbed(execResult.Exception.Message,
                    Color.Red));

                await Task.Delay(_configuration.DeleteErrorReplyDelay);

                await reply.DeleteAsync();
            }
            else if (result.Error != CommandError.UnknownCommand)
            {
                _logger.LogWarning(
                    $"Error ({result.Error}) occurred while executing command {message.Content} - {result.ErrorReason}");

                var reply = await message.Channel.SendMessageAsync(
                    embed: EmbedHelper.SimpleEmbed($"An error occurred while executing this command ({result.Error}).",
                        Color.Red));

                await Task.Delay(_configuration.DeleteErrorReplyDelay);

                await reply.DeleteAsync();
            }
        }
    }
}
