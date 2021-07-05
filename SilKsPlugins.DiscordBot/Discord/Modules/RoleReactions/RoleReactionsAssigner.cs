using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SilKsPlugins.DiscordBot.Components;
using SilKsPlugins.DiscordBot.Discord.Modules.RoleReactions.Services.API;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Modules.RoleReactions
{
    public class RoleReactionsAssigner : IComponent
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public RoleReactionsAssigner(DiscordSocketClient discordClient,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _discordClient = discordClient;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _discordClient.ReactionAdded += OnReactionAddedAsync;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _discordClient.ReactionAdded -= OnReactionAddedAsync;

            return Task.CompletedTask;
        }
        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cacheableMessage, ISocketMessageChannel messageChannel, SocketReaction reaction)
        {
            // Check if reaction is from bot
            if (_discordClient.CurrentUser.Id == reaction.UserId)
            {
                return;
            }

            if (messageChannel is not SocketTextChannel textChannel)
            {
                return;
            }

            var isSubscribing = reaction.Emote.Name == _configuration["RoleReactions:Subscribe"];
            var isUnsubscribing = reaction.Emote.Name == _configuration["RoleReactions:Unsubscribe"];

            // Check if emote means anything
            if (!isSubscribing && !isUnsubscribing)
            {
                return;
            }

            var roleReactionManager = _serviceProvider.GetRequiredService<IRoleReactionDatabaseManager>();

            var roleId =
                await roleReactionManager.GetRoleForMessage(textChannel.Guild.Id, textChannel.Id, cacheableMessage.Id);

            if (roleId == null)
            {
                return;
            }

            var role = textChannel.Guild.GetRole(roleId.Value);

            if (role == null)
            {
                return;
            }

            var user = textChannel.Guild.GetUser(reaction.UserId) ??
                       throw new Exception("Could not find user for reaction: " + reaction.UserId);

            if (isSubscribing)
            {
                await user.AddRoleAsync(role);
            }
            else
            {
                await user.RemoveRoleAsync(role);
            }

            var message = await cacheableMessage.GetOrDownloadAsync();

            await message.RemoveReactionAsync(reaction.Emote, reaction.UserId);
        }
    }
}
