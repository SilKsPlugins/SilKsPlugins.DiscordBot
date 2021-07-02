using Discord.Commands;
using JetBrains.Annotations;
using SilKsPlugins.DiscordBot.Databases.Plugins.Models;
using SilKsPlugins.DiscordBot.Discord.Commands;
using SilKsPlugins.DiscordBot.Discord.Modules.Plugins.Services;
using SilKsPlugins.DiscordBot.Discord.Preconditions;
using System;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Modules.Plugins
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CategoriesModule : CustomModuleBase<SocketCommandContext>
    {
        private readonly PluginManager _pluginManager;
        private readonly CommandConfigAccessor _commandConfigAccessor;

        public CategoriesModule(
            PluginManager pluginManager,
            CommandConfigAccessor commandConfigAccessor,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _pluginManager = pluginManager;
            _commandConfigAccessor = commandConfigAccessor;
        }

        [Command("categories help")]
        [Alias("categories ?", "categories")]
        [Summary("View help for the categories command.")]
        [RequireBotAdmin]
        public async Task CategoriesHelpAsync()
        {
            var prefix = _commandConfigAccessor.CommandPrefix;
            
            await ReplyAsync($@"
{prefix}categories help - View help for the '{prefix}categories' command.
{prefix}categories list [brief | verbose] - View all categories.
{prefix}categories add <id> <title> [category id] - Add a category.
{prefix}categories remove <id> - Remove a category.
{prefix}categories edit title <id> <title> - Edit the title of a category.
{prefix}categories edit category <id> [category id] - Edit the channel category of a category. Specify no channel category id to remove.
            ");
        }

        [Command("categories list")]
        [Summary("View all categories.")]
        [RequireBotAdmin]
        public async Task CategoriesListAsync(string detail = "brief")
        {
            var categories = await _pluginManager.GetCategories();

            string response;

            if (categories.Count == 0)
            {
                response = "No categories.";
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
                    response = "**Categories:**";

                    foreach (var category in categories)
                    {
                        response += $"\n- {category.Title}";
                    }
                }
                else
                {
                    response = "**Categories (Verbose):**";

                    foreach (var category in categories)
                    {
                        response += $"\n- Id: {category.Id}";
                        response += $"\n  Title: {category.Title}";
                        response += $"\n  Channel Category Id: {category.ChannelCategoryId?.ToString() ?? "null"}";
                    }
                }
            }

            await ReplyAsync(response);
        }

        [Command("categories add")]
        [Summary("Add a category.")]
        [RequireBotAdmin]
        public async Task CategoriesAddAsync(string id, string title, ulong? categoryId = null)
        {
            var category = new CategoryInfo
            {
                Id = id,
                Title = title,
                ChannelCategoryId = categoryId
            };

            if (await _pluginManager.AddCategory(category))
            {
                await ReplyAndDeleteAsync($"Successfully added category {category.Id}.");
            }
            else
            {
                await ReplyAndDeleteAsync("This category already exists.");
            }
        }

        [Command("categories remove")]
        [Summary("Remove a category.")]
        [RequireBotAdmin]
        public async Task CategoriesRemoveAsync(string id)
        {
            if (await _pluginManager.RemoveCategory(id))
            {
                await ReplyAndDeleteAsync($"Successfully removed category {id}.");
            }
            else
            {
                await ReplyAndDeleteAsync("This category does not exist.");
            }
        }

        [Command("categories edit")]
        [Summary("Edit properties of categories.")]
        [RequireBotAdmin]
        public Task CategoriesEditAsync()
        {
            return Task.FromException(
                new UserFriendlyException(
                    "Specify which property you'd like to edit. Use the help command to view more info."));
        }

        [Command("categories edit title")]
        [Summary("Edit the title of a category.")]
        [RequireBotAdmin]
        public async Task CategoriesEditTitleAsync(string id, string title)
        {
            if (await _pluginManager.UpdateCategory(id, category => category.Title = title))
            {
                await ReplyAndDeleteAsync($"Successfully changed {id}'s title to {title}.");
            }
            else
            {
                await ReplyAndDeleteAsync($"The category {id} does not exists.");
            }
        }

        [Command("categories edit category")]
        [Summary("Edit the channel category of a category. Specify no channel category id to remove.")]
        [RequireBotAdmin]
        public async Task CategoriesEditCategoryAsync(string id, ulong? categoryId = null)
        {
            if (await _pluginManager.UpdateCategory(id, category => category.ChannelCategoryId = categoryId))
            {
                await ReplyAndDeleteAsync($"Successfully changed {id}'s channel category ID to {categoryId}.");
            }
            else
            {
                await ReplyAndDeleteAsync($"The category {id} does not exists.");
            }
        }
    }
}
