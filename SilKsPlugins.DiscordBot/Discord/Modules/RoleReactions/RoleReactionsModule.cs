using Discord;
using Discord.Commands;
using JetBrains.Annotations;
using SilKsPlugins.DiscordBot.Discord.Commands;
using SilKsPlugins.DiscordBot.Discord.Modules.RoleReactions.Services;
using SilKsPlugins.DiscordBot.Discord.Preconditions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Modules.RoleReactions
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class RoleReactionsModule : CustomModuleBase<SocketCommandContext>
    {
        private readonly IRoleReactionDatabaseManager _roleReactionDatabaseManager;
        private readonly IRoleReactionMessageManager _roleReactionMessageManager;

        public RoleReactionsModule(IRoleReactionDatabaseManager roleReactionDatabaseManager,
            IRoleReactionMessageManager roleReactionMessageManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _roleReactionDatabaseManager = roleReactionDatabaseManager;
            _roleReactionMessageManager = roleReactionMessageManager;
        }

        [Command("addrolereaction")]
        [Summary("Manage role reactions for the given message.")]
        [RequireBotAdmin]
        public async Task AddRoleReactionAsync(ulong messageId, string roleNameOrId)
        {
            var message = await Context.Channel.GetMessageAsync(messageId) ??
                          throw new UserFriendlyException("Message not found.");

            var setupRoleId =
                await _roleReactionDatabaseManager.GetRoleForMessage(Context.Guild.Id, Context.Channel.Id, message.Id);

            if (setupRoleId != null)
            {
                throw new UserFriendlyException("This message already has a role reaction setup.");
            }

            IRole? role = null;
            var shouldCreateIfNotExists = true;

            if (ulong.TryParse(roleNameOrId, out var roleId))
            {
                role = Context.Guild.GetRole(roleId);
                shouldCreateIfNotExists = false;
            }

            role ??= Context.Guild.Roles.FirstOrDefault(x =>
                         x.Name.Equals(roleNameOrId, StringComparison.OrdinalIgnoreCase));
            var createdRole = false;

            if (role == null && shouldCreateIfNotExists)
            {
                // isMentionable is required to choose otherwise ambiguous method
                role = await Context.Guild.CreateRoleAsync(roleNameOrId, isMentionable: false);
            }

            if (role == null)
            {
                throw new UserFriendlyException("Role not found.");
            }

            if (await _roleReactionDatabaseManager.AddRoleToMessage(Context.Guild.Id, Context.Channel.Id, message.Id, role.Id))
            {
                await _roleReactionMessageManager.AddReactionToMessage(message);

                await ReplyAndDeleteAsync(createdRole
                    ? $"This message has been set up for role reactions and the role {role.Name} has been created."
                    : $"This message has been set up for role reactions for the role {role.Name}.");
            }
            else
            {
                if (createdRole)
                {
                    await role.DeleteAsync();
                }

                throw new UserFriendlyException("Could not add role to message.");
            }
        }

        [Command("removerolereaction")]
        [Summary("Remove the role reaction for the given message.")]
        [RequireBotAdmin]
        public async Task RemoveRoleReactionAsync(ulong messageId)
        {
            var message = await Context.Channel.GetMessageAsync(messageId) ??
                          throw new UserFriendlyException("Message not found.");

            if (await _roleReactionDatabaseManager.RemoveRoleFromMessage(Context.Guild.Id, Context.Channel.Id, message.Id))
            {
                await _roleReactionMessageManager.RemoveReactionFromMessage(message);

                await ReplyAndDeleteAsync("Successfully removed role reaction from the given message.");
            }
            else
            {
                throw new UserFriendlyException("The given message does not have a role reaction.");
            }
        }
    }
}
