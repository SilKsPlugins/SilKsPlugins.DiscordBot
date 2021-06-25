using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SilKsPlugins.DiscordBot.Helpers;
using System;
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
            Commands.CommandExecuted += OnCommandExecuted;
            
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (result.IsSuccess)
            {
                _logger.LogDebug(
                    $"Successfully executed command {context.Message.Content}.");
            }
            else if (result is ExecuteResult
            {
                Error: CommandError.Exception,
                Exception: UserFriendlyException
            } execResult)
            {
                var reply = await context.Message.Channel.SendMessageAsync(
                    embed: EmbedHelper.SimpleEmbed(execResult.Exception.Message, Color.Red));

                _logger.LogDebug(
                    $"Successfully executed command {context.Message.Content} ({nameof(UserFriendlyException)}).");

                await Task.Delay(_configuration.DeleteErrorReplyDelay);

                await reply.DeleteAsync();
            }
            else if (result.Error != CommandError.UnknownCommand)
            {
                var exception = (result as ExecuteResult?)?.Exception;

                if (exception != null)
                {

                    _logger.LogError(exception,
                        $"Error ({result.Error}) occurred while executing command {context.Message.Content} - {result.ErrorReason}");
                }
                else
                {
                    _logger.LogError(
                        $"Error ({result.Error}) occurred while executing command {context.Message.Content} - {result.ErrorReason}");
                }

                var reply = await context.Channel.SendMessageAsync(
                    embed: EmbedHelper.SimpleEmbed($"An error occurred while executing this command ({result.Error}).",
                        Color.Red));

                await Task.Delay(_configuration.DeleteErrorReplyDelay);

                await reply.DeleteAsync();
            }
        }

        public void Dispose()
        {
            _client.MessageReceived -= HandleCommandAsync;
            Commands.CommandExecuted -= OnCommandExecuted;
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
            
            await Commands.ExecuteAsync(context, argPos, _services);
        }
    }
}
