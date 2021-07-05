using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JetBrains.Annotations;
using Serilog;
using SilKsPlugins.DiscordBot.Databases.Plugins.Models;
using SilKsPlugins.DiscordBot.Discord.Commands;
using SilKsPlugins.DiscordBot.Discord.Modules.Plugins.Services;
using SilKsPlugins.DiscordBot.Discord.Modules.RoleReactions.Services.API;
using SilKsPlugins.DiscordBot.Discord.Preconditions;
using SilKsPlugins.DiscordBot.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Modules.Plugins
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PluginsModule : CustomModuleBase<SocketCommandContext>
    {
        private readonly PluginManager _pluginManager;
        private readonly CommandConfigAccessor _commandConfigAccessor;
        private readonly IRoleReactionDatabaseManager _roleReactionDatabaseManager;
        private readonly IRoleReactionMessageManager _roleReactionMessageManager;


        public PluginsModule(
            PluginManager pluginManager,
            CommandConfigAccessor commandConfigAccessor,
            IRoleReactionDatabaseManager roleReactionDatabaseManager,
            IRoleReactionMessageManager roleReactionMessageManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _pluginManager = pluginManager;
            _commandConfigAccessor = commandConfigAccessor;
            _roleReactionDatabaseManager = roleReactionDatabaseManager;
            _roleReactionMessageManager = roleReactionMessageManager;
        }
        
        [Command("plugins help")]
        [Alias("plugins ?", "plugins")]
        [Summary("View help for the plugins command.")]
        [RequireBotAdmin]
        public async Task PluginsHelpAsync()
        {
            var prefix = _commandConfigAccessor.CommandPrefix;

            await ReplyAsync($@"
`{prefix}plugins help` - View help for the '{prefix}plugins' command.
`{prefix}plugins output [category] [plugin]` - Outputs all plugins information to defined/new channels.
`{prefix}plugins list [brief | verbose] [category]` - View all plugins.
`{prefix}plugins add <id> <description> <category> [description]` - Add a plugin.
`{prefix}plugins remove <id>` - Remove a plugin.
`{prefix}plugins edit title <id> <title>` - Edit the title of a plugin.
`{prefix}plugins edit description <id>` [description] - Edit the description of a plugin. Specify none to remove.
`{prefix}plugins edit url <id> [url]` - Edit the url of a plugin. Specify none to remove.
`{prefix}plugins edit price <id> [price]` - Edit the price of a plugin. Specify none to remove.
`{prefix}plugins edit channel <id> [channel]` - Edit the channel of a plugin. Specify none to remove.
`{prefix}plugins edit author <id> [author]` - Edit the author of a plugin. Specify none to remove.
`{prefix}plugins edit category <id> <category>` - Edit the category of a plugin.
`{prefix}plugins edit content <id>` - Edit the content of a plugin.
            ");
        }

        private Embed GetPluginEmbedAsync(PluginInfo plugin)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle(plugin.Title);

            var descriptionParts = new List<string>();

            if (plugin.Price != null && plugin.Price.Value != 0)
            {
                descriptionParts.Add($"Price: **{plugin.Price:C} USD**");
            }

            if ((plugin.Platforms & PluginPlatform.All) != 0)
            {
                var platforms = new List<string>();

                if ((plugin.Platforms & PluginPlatform.OpenMod) != 0)
                {
                    platforms.Add("OpenMod");
                }

                if ((plugin.Platforms & PluginPlatform.RocketMod) != 0)
                {
                    platforms.Add("RocketMod");
                }

                descriptionParts.Add($"Platforms: **{string.Join(", ", platforms)}**");
            }

            if (descriptionParts.Count > 0)
            {
                embedBuilder.WithDescription(string.Join(" | ", descriptionParts));
            }

            if (plugin.Url != null)
            {
                embedBuilder.WithUrl(plugin.Url);
            }

            if (plugin.Author != null)
            {
                embedBuilder.WithAuthor(plugin.Author);
            }

            if (plugin.CreationTime != null)
            {
                embedBuilder.WithTimestamp(plugin.CreationTime.Value);
            }

            if (plugin.IconUrl != null)
            {
                embedBuilder.WithThumbnailUrl(plugin.IconUrl);
            }
            
            if (plugin.Content != null)
            {
                var contentLines = plugin.Content.Split("\n").Select(x => x.TrimEnd()).ToArray();

                for (var i = 0; i < contentLines.Length; i++)
                {
                    var header = $"**{contentLines[i].TrimStart('#').TrimStart()}**";

                    var stringBuilder = new StringBuilder();

                    for (i++; i < contentLines.Length; i++)
                    {
                        if (contentLines[i].StartsWith('#'))
                        {
                            i--;
                            break;
                        }

                        if (stringBuilder.Length + contentLines[i].Length > 1024)
                        {
                            Log.Logger.Debug(header);
                            embedBuilder.AddFieldSafe(header, stringBuilder.ToString());
                            header = "\a";
                            stringBuilder = new StringBuilder();
                        }

                        stringBuilder.AppendLine(contentLines[i]);
                    }

                    embedBuilder.AddFieldSafe(header, stringBuilder.ToString());
                }
            }

            return embedBuilder.Build();
        }

        [Command("plugins output")]
        [Summary("Outputs all plugins information to defined/new channels.")]
        [RequireBotAdmin]
        public async Task PluginsOutputAsync(string? categoryFilter = null, string? pluginFilter = null)
        {
            var output = "**Actions:**";

            void RunAction(string description)
            {
                output += $"\n- {description}";
            }

            var categories = await _pluginManager.GetCategories();

            foreach (var category in categories)
            {
                if (categoryFilter != null && !Regex.IsMatch(category.Id, categoryFilter))
                {
                    continue;
                }

                ICategoryChannel? categoryChannel = null;
                string categoryName;

                if (category.ChannelCategoryId != null)
                {
                    categoryChannel = Context.Guild.GetCategoryChannel(category.ChannelCategoryId.Value);
                }

                if (categoryChannel == null)
                {
                    categoryName = category.Title.Replace(' ', '-').ToLower();

                    RunAction($"Create category channel with title `{categoryName}` for category `{category.Id}`.");
                    categoryChannel = await Context.Guild.CreateCategoryChannelAsync(categoryName);

                    RunAction($"Set channel category for plugin `{category.Id}` to `{categoryName}`.");
                    await _pluginManager.UpdateCategory(category.Id,
                        pluginUpdate => pluginUpdate.ChannelCategoryId = categoryChannel.Id);
                }
                else
                {
                    categoryName = categoryChannel.Name;
                }

                RunAction($"Set channel category `{categoryName}` permission overwrite to deny sending messages and adding reactions for everyone");
                var everyoneRole = Context.Guild.EveryoneRole;
                await categoryChannel.AddPermissionOverwriteAsync(everyoneRole,
                    new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny));

                foreach (var plugin in category.Plugins)
                {
                    if (pluginFilter != null && !Regex.IsMatch(plugin.Id, pluginFilter))
                    {
                        continue;
                    }

                    ITextChannel? pluginTextChannel = null;
                    string pluginChannelName;

                    if (plugin.ChannelId != null)
                    {
                        pluginTextChannel = Context.Guild.GetTextChannel(plugin.ChannelId.Value);
                    }

                    if (pluginTextChannel == null)
                    {
                        pluginChannelName = plugin.Title.Replace(' ', '-').ToLower();

                        RunAction(
                            $"Create text channel with title `{pluginChannelName}` for plugin `{plugin.Id}` in channel category `{categoryName}`.");
                        pluginTextChannel = await Context.Guild.CreateTextChannelAsync(pluginChannelName,
                            properties => properties.CategoryId = categoryChannel.Id);

                        RunAction($"Set text channel for plugin `{plugin.Id}` to `{pluginChannelName}`.");
                        await _pluginManager.UpdatePlugin(plugin.Id,
                            pluginUpdate => pluginUpdate.ChannelId = pluginTextChannel.Id);
                    }
                    else
                    {
                        pluginChannelName = pluginTextChannel.Name;
                    }

                    var pinnedMessages = await pluginTextChannel.GetPinnedMessagesAsync();

                    var message =
                        pinnedMessages.FirstOrDefault(x => x.Author.Id == Context.Client.CurrentUser.Id) as IUserMessage;

                    var embed = GetPluginEmbedAsync(plugin);

                    if (message == null)
                    {
                        RunAction($"Send embed for plugin `{plugin.Id}` to `{pluginChannelName}` channel and pin message.");
                        message = await pluginTextChannel.SendMessageAsync(embed: embed);

                        await message.PinAsync();

                        var pinnedMessageNotification = await pluginTextChannel.GetMessagesAsync(10).Flatten().Where(x =>
                                x.Author.Id == Context.Client.CurrentUser.Id && x.Type == MessageType.ChannelPinnedMessage)
                            .FirstOrDefaultAsync();

                        if (pinnedMessageNotification != null)
                        {
                            await pinnedMessageNotification.DeleteAsync();
                        }
                    }
                    else
                    {
                        RunAction($"Edit existing embed for plugin `{plugin.Id}` in `{pluginChannelName}` channel.");
                        await message.ModifyAsync(properties => properties.Embed = embed);
                    }

                    var role = pluginTextChannel.Guild.Roles.FirstOrDefault(x =>
                        x.Name.Equals(plugin.Title, StringComparison.OrdinalIgnoreCase));

                    if (role == null)
                    {
                        RunAction($"Create role '{plugin.Title}' for reaction roles.");
                        role = await pluginTextChannel.Guild.CreateRoleAsync(plugin.Title, isMentionable: false);
                    }

                    RunAction($"Remove all reactions on plugin embed `{plugin.Id}` in `{pluginChannelName}` channel.");
                    await message.RemoveAllReactionsAsync();

                    RunAction($"Add subscription reactions to plugin embed `{plugin.Id}` in `{pluginChannelName}` channel.");
                    await _roleReactionMessageManager.AddReactionToMessage(message);

                    RunAction($"Add role reactions for plugin embed `{plugin.Id}` to database.");
                    await _roleReactionDatabaseManager.AddRoleToMessage(pluginTextChannel.GuildId, pluginTextChannel.Id,
                        message.Id, role.Id);
                }
            }

            output += "\n";

            var builder = new StringBuilder();

            foreach (var line in output.Split('\n').Select(x => x.TrimEnd()))
            {
                if (builder.Length + line.Length > 1996)
                {
                    await (await ReplyAsync(builder.ToString())).ModifySuppressionAsync(true);
                    builder = new StringBuilder();
                }

                builder.AppendLine(line);
            }

            await (await ReplyAsync(builder.ToString())).ModifySuppressionAsync(true);
        }

        [Command("plugins fakeoutput")]
        [Summary("Replies with all actions that will occur if the 'plugins output' command was executed.")]
        [RequireBotAdmin]
        public async Task PluginsFakeOutputAsync(string? categoryFilter = null, string? pluginFilter = null)
        {
            var output = "**Actions (Not-Executed):**";

            void RunAction(string description)
            {
                output += $"\n- {description}";
            }

            var categories = await _pluginManager.GetCategories();

            foreach (var category in categories)
            {
                if (categoryFilter != null && !Regex.IsMatch(category.Id, categoryFilter))
                {
                    continue;
                }

                ICategoryChannel? categoryChannel = null;
                string categoryName;

                if (category.ChannelCategoryId != null)
                {
                    categoryChannel = Context.Guild.GetCategoryChannel(category.ChannelCategoryId.Value);
                }

                if (categoryChannel == null)
                {
                    categoryName = category.Title.Replace(' ', '-').ToLower();

                    RunAction($"Create category channel with title `{categoryName}` for category `{category.Id}`.");
                    //categoryChannel = await Context.Guild.CreateCategoryChannelAsync(categoryName);

                    RunAction($"Set channel category for plugin `{category.Id}` to `{categoryName}`.");
                    //await _pluginManager.UpdateCategory(category.Id,
                    //    pluginUpdate => pluginUpdate.ChannelCategoryId = categoryChannel.Id);
                }
                else
                {
                    categoryName = categoryChannel.Name;
                }

                RunAction($"Set channel category `{categoryName}` permission overwrite to deny sending messages and adding reactions for everyone");
                //var everyoneRole = Context.Guild.EveryoneRole;
                //await categoryChannel.AddPermissionOverwriteAsync(everyoneRole,
                //    new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny));

                foreach (var plugin in category.Plugins)
                {
                    if (pluginFilter != null && !Regex.IsMatch(plugin.Id, pluginFilter))
                    {
                        continue;
                    }

                    ITextChannel? pluginTextChannel = null;
                    string pluginChannelName;

                    if (plugin.ChannelId != null)
                    {
                        pluginTextChannel = Context.Guild.GetTextChannel(plugin.ChannelId.Value);
                    }

                    if (pluginTextChannel == null)
                    {
                        pluginChannelName = plugin.Title.Replace(' ', '-').ToLower();

                        RunAction(
                            $"Create text channel with title `{pluginChannelName}` for plugin `{plugin.Id}` in channel category `{categoryName}`.");
                        //pluginTextChannel = await Context.Guild.CreateTextChannelAsync(pluginChannelName,
                        //    properties => properties.CategoryId = categoryChannel.Id);

                        RunAction($"Set text channel for plugin `{plugin.Id}` to `{pluginChannelName}`.");
                        //await _pluginManager.UpdatePlugin(plugin.Id,
                        //    pluginUpdate => pluginUpdate.ChannelId = pluginTextChannel.Id);
                    }
                    else
                    {
                        pluginChannelName = pluginTextChannel.Name;
                    }

                    var pinnedMessages = pluginTextChannel == null
                        ? null
                        : await pluginTextChannel.GetPinnedMessagesAsync();

                    var message =
                        pinnedMessages?.FirstOrDefault(x => x.Author.Id == Context.Client.CurrentUser.Id) as IUserMessage;

                    //var embed = GetPluginEmbedAsync(plugin);

                    if (message == null)
                    {
                        RunAction($"Send embed for plugin `{plugin.Id}` to `{pluginChannelName}` channel and pin message.");
                        //var newMessage = await pluginTextChannel.SendMessageAsync(embed: embed);

                        //await newMessage.PinAsync();

                        //var pinnedMessageNotification = await pluginTextChannel.GetMessagesAsync(10).Flatten().Where(x =>
                        //        x.Author.Id == Context.Client.CurrentUser.Id && x.Type == MessageType.ChannelPinnedMessage)
                        //    .FirstOrDefaultAsync();

                        //if (pinnedMessageNotification != null)
                        //{
                        //    await pinnedMessageNotification.DeleteAsync();
                        //}
                    }
                    else
                    {
                        RunAction($"Edit existing embed for plugin `{plugin.Id}` in `{pluginChannelName}` channel.");
                        //await message.ModifyAsync(properties => properties.Embed = embed);
                    }
                }
            }

            var builder = new StringBuilder();

            foreach (var line in output.Split('\n').Select(x => x.TrimEnd()))
            {
                if (builder.Length + line.Length > 1996)
                {
                    await (await ReplyAsync(builder.ToString())).ModifySuppressionAsync(true);
                    builder = new StringBuilder();
                }

                builder.AppendLine(line);
            }

            await (await ReplyAsync(builder.ToString())).ModifySuppressionAsync(true);
        }

        [Command("plugins list")]
        [Summary("View all plugins.")]
        public async Task PluginsListAsync(string detail = "brief", string? category = null)
        {
            var plugins = (await _pluginManager.GetPlugins()).ToList();

            if (category != null)
            {
                plugins.RemoveAll(x => !x.CategoryId.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            string response;

            if (plugins.Count == 0)
            {
                response = "No plugins.";
            }
            else
            {
                var isBrief = detail.ToLower() switch
                {
                    "brief" => true,
                    "verbose" => false,
                    _ => throw new UserFriendlyException($"Unknown level of detail: {detail}")
                };

                if (isBrief)
                {
                    response = "**Plugins:**";

                    foreach (var plugin in plugins)
                    {
                        response += $"\n- {plugin.Title}";

                        if (plugin.ChannelId != null)
                        {
                            response += $" <#{plugin.ChannelId}>";
                        }

                        if (plugin.Description != null)
                        {
                            response += $" - {plugin.Description}";
                        }
                    }
                }
                else
                {
                    response = "**Plugins (Verbose):**";

                    foreach (var plugin in plugins)
                    {
                        response += $@"
- Id: {plugin.Id}
  Title: {plugin.Title}
  Description: {plugin.Description ?? "null"}
  Url: {plugin.Url ?? "null"}
  Price: {plugin.Price?.ToString() ?? "null"}
  ChannelId: {plugin.ChannelId?.ToString() ?? "null"}
  Author: {plugin.Author ?? "null"}
  CategoryId: {plugin.Category.Id}
  CategoryTitle: {plugin.Category.Title}
                        ";
                    }
                }
            }

            var builder = new StringBuilder();

            foreach (var line in response.Split('\n').Select(x => x.TrimEnd()))
            {
                if (builder.Length + line.Length > 1996)
                {
                    await (await ReplyAsync(builder.ToString())).ModifySuppressionAsync(true);
                    builder = new StringBuilder();
                }

                builder.AppendLine(line);
            }

            await (await ReplyAsync(builder.ToString())).ModifySuppressionAsync(true);
        }

        [Command("plugins add")]
        [Summary("Add a plugin.")]
        [RequireBotAdmin]
        public async Task PluginsAddAsync(string id, string title, string categoryId, string? description = null)
        {
            var category = await _pluginManager.GetCategory(categoryId) ??
                           throw new UserFriendlyException($"Category {categoryId} could not be found.");

            var plugin = new PluginInfo
            {
                Id = id,
                Title = title,
                Description = description,
                Category = category
            };

            if (await _pluginManager.AddPlugin(plugin))
            {
                await ReplyAndDeleteAsync($"Successfully added plugin {plugin.Id}.");
            }
            else
            {
                await ReplyAndDeleteAsync("This plugin already exists.");
            }
        }

        [Command("plugins remove")]
        [Summary("Remove a plugin.")]
        [RequireBotAdmin]
        public async Task PluginsRemoveAsync(string id)
        {
            if (await _pluginManager.RemovePlugin(id))
            {
                await ReplyAndDeleteAsync($"Successfully removed plugin {id}.");
            }
            else
            {
                await ReplyAndDeleteAsync("This plugin does not exist.");
            }
        }

        private async Task EditPluginAsync(string id, Action<PluginInfo> editQuery)
        {
            if (!await _pluginManager.UpdatePlugin(id, editQuery))
            {
                throw new UserFriendlyException($"The plugin {id} does not exists.");
            }
        }

        [Command("plugins edit")]
        [Summary("Edit properties of plugins.")]
        [RequireBotAdmin]
        public Task PluginsEditAsync()
        {
            return Task.FromException(
                new UserFriendlyException(
                    "Specify which property you'd like to edit. Use the help command to view more info."));
        }

        [Command("plugins edit title")]
        [Summary("Edit the title of a plugin.")]
        [RequireBotAdmin]
        public async Task PluginsEditTitleAsync(string id, string title)
        {
            await EditPluginAsync(id, plugin => plugin.Title = title);

            await ReplyAndDeleteAsync($"Successfully changed {id}'s title to {title}.");
        }

        [Command("plugins edit description")]
        [Summary("Edit the description of a plugin.")]
        [RequireBotAdmin]
        public async Task PluginsEditDescriptionAsync(string id, string? description)
        {
            await EditPluginAsync(id, plugin => plugin.Description = description);

            await ReplyAndDeleteAsync($"Successfully changed {id}'s description to {description ?? "null"}.");
        }

        [Command("plugins edit url")]
        [Summary("Edit the url of a plugin.")]
        [RequireBotAdmin]
        public async Task PluginsEditUrlAsync(string id, string? url)
        {
            await EditPluginAsync(id, plugin => plugin.Url = url);

            await ReplyAndDeleteAsync($"Successfully changed {id}'s url to {url ?? "null"}.");
        }

        [Command("plugins edit price")]
        [Summary("Edit the price of a plugin.")]
        [RequireBotAdmin]
        public async Task PluginsEditPriceAsync(string id, decimal? price)
        {
            await EditPluginAsync(id, plugin => plugin.Price = price);

            await ReplyAndDeleteAsync($"Successfully changed {id}'s price to {price?.ToString() ?? "null"}.");
        }

        [Command("plugins edit channel")]
        [Summary("Edit the channel of a plugin.")]
        [RequireBotAdmin]
        public async Task PluginsEditChannelAsync(string id, string? channel)
        {
            ulong? channelId = null;

            if (channel != null)
            {
                channel = channel.TrimStart('<', '#').TrimEnd('>');

                channelId = ulong.TryParse(channel, out var parsedChannelId)
                    ? parsedChannelId
                    : throw new UserFriendlyException($"Could not parse channel id from {channel}.");
            }

            await _pluginManager.UpdatePlugin(id, plugin => plugin.ChannelId = channelId);

            channel = channelId == null ? "null" : $"<#{channelId}>";

            await ReplyAndDeleteAsync($"Successfully changed {id}'s channel to {channel}.");
        }

        [Command("plugins edit author")]
        [Summary("Edit the author of a plugin.")]
        [RequireBotAdmin]
        public async Task PluginsEditAuthorAsync(string id, string? author)
        {
            await EditPluginAsync(id, plugin => plugin.Author = author);

            await ReplyAndDeleteAsync($"Successfully changed {id}'s author to {author ?? "null"}.");
        }

        [Command("plugins edit category")]
        [Summary("Edit the category of a plugin.")]
        [RequireBotAdmin]
        public async Task PluginsEditCategoryAsync(string id, string categoryId)
        {
            var category = await _pluginManager.GetCategory(categoryId) ??
                           throw new UserFriendlyException($"Category {categoryId} could not be found.");

            await EditPluginAsync(id, plugin => plugin.Category = category);

            await ReplyAndDeleteAsync($"Successfully changed {id}'s category to {categoryId}.");
        }

        private class EditContentCallback
        {
            private readonly SocketCommandContext _commandContext;
            private readonly IUserMessage _watchedMessage;

            private readonly TaskCompletionSource<string?> _contentTcs;

            public EditContentCallback(SocketCommandContext commandContext,
                IUserMessage watchedMessage)
            {
                _commandContext = commandContext;
                _watchedMessage = watchedMessage;

                _contentTcs = new TaskCompletionSource<string?>();
            }

            private async Task OnMessageReceivedAsync(SocketMessage message)
            {
                if (message is not SocketUserMessage userMessage)
                {
                    return;
                }
                
                if (userMessage.ReferencedMessage == null
                    || userMessage.ReferencedMessage.Id != _watchedMessage.Id)
                {
                    return;
                }

                if (message.Author.Id != _commandContext.User.Id)
                {
                    return;
                }

                string? content;

                var attachment = message.Attachments.FirstOrDefault();

                if (attachment != null)
                {
                    // 16 kilobytes
                    const int maxAttachmentSize = 16 * 1024;

                    if (attachment.Size > maxAttachmentSize)
                    {
                        await message.Channel.SendMessageAsync(
                            embed: EmbedHelper.SimpleEmbed($"File is too large (max size is {maxAttachmentSize} bytes)",
                                Color.Red));

                        return;
                    }

                    using var client = new HttpClient();

                    content = await client.GetStringAsync(attachment.Url);
                }
                else
                {
                    content = message.Content;
                }

                if (string.IsNullOrWhiteSpace(content) || content == "null")
                {
                    content = null;
                }

                _contentTcs.SetResult(content);
            }

            public async Task<string?> GetNewContent(TimeSpan timeout)
            {
                try
                {
                    var cancellationToken = new CancellationTokenSource(timeout);

                    cancellationToken.Token.Register(() => _contentTcs.TrySetCanceled(),
                        useSynchronizationContext: false);
                    
                    _commandContext.Client.MessageReceived += OnMessageReceivedAsync;

                    return await _contentTcs.Task;
                }
                finally
                {
                    _commandContext.Client.MessageReceived -= OnMessageReceivedAsync;
                }
            }
        }

        [Command("plugins edit content")]
        [Summary("Edit the content of a plugin.")]
        [RequireBotAdmin]
        public async Task PluginsEditContentAsync(string id)
        {
            var plugin = await _pluginManager.GetPlugin(id) ??
                         throw new UserFriendlyException($"The plugin {id} does not exists.");

            IUserMessage[] replies;

            if (plugin.Content != null)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                await writer.WriteAsync(plugin.Content);
                await writer.FlushAsync();
                stream.Position = 0;

                replies = new []
                {
                    await ReplyAsync(
                        "Reply to this message with the edited content for the plugin (reply `null` to set content to null).\nYou have five minutes to reply."),
                    await Context.Channel.SendFileAsync(stream, "PluginContent.txt",
                        "The current content of the plugin.")
                };
            }
            else
            {
                replies = new[]
                {
                    await ReplyAsync(
                        "Reply to this message with the new content for the plugin (reply `null` to set content to null).\nYou have five minutes to reply.")
                };
            }

            var timeout = TimeSpan.FromMinutes(5);

            var callback = new EditContentCallback(Context, replies[0]);

            try
            {
                var newContent = await callback.GetNewContent(timeout);

                await _pluginManager.UpdatePlugin(plugin.Id, updatePlugin => updatePlugin.Content = newContent);

                await ReplyAndDeleteAsync($"Content was successfully edited for plugin {plugin.Id}.");

                foreach (var reply in replies)
                {
                    await reply.DeleteAsync();
                }
            }
            catch (OperationCanceledException)
            {
                await ReplyAndDeleteAsync(
                    embed: EmbedHelper.SimpleEmbed(
                        "Five minutes have been surpassed - the content of the plugin was not edited.", Color.Red),
                    messageReference: Context.Message.Reference);

                foreach (var reply in replies)
                {
                    await reply.DeleteAsync();
                }
            }
        }
    }
}
