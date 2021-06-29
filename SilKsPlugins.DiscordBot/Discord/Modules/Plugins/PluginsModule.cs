using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ImperialPlugins.Tools.Api;
using ImperialPlugins.Tools.Client;
using ImperialPlugins.Tools.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Serilog;
using SilKsPlugins.DiscordBot.Discord.Commands;
using SilKsPlugins.DiscordBot.Discord.Modules.Plugins.Services;
using SilKsPlugins.DiscordBot.Discord.Preconditions;
using SilKsPlugins.DiscordBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Modules.Plugins
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PluginsModule : CustomModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;
        private readonly MerchantIdAccessor _merchantIdAccessor;

        public PluginsModule(IConfiguration configuration,
            MerchantIdAccessor merchantIdAccessor,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configuration = configuration;
            _merchantIdAccessor = merchantIdAccessor;
        }

        private Configuration GetApiConfig()
        {
            var config = new Configuration();

            config.ApiKey.Add("X-API-KEY", _configuration["ImperialPluginsApiKey"]);

            return config;
        }

        [Command("getmerchantid")]
        [Summary("Gets the given merchant's ID.")]
        [RequireBotAdmin]
        public async Task GetMerchantIdAsync(string merchantName)
        {
            var config = GetApiConfig();

            var merchantsApi = new MerchantsApi();

            var merchants = (await merchantsApi.GetMerchantsAsync(maxResultCount: 200)).Items
                .Where(x => x.Name.Contains(merchantName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var message = string.Join("\n", merchants.Select(x => $"{x.Name} - {x.Id}"));

            await ReplyAndDeleteAsync(message);
        }

        [Command("getcurrentmerchantid")]
        [Summary("Gets the configured merchant ID used by the bot.")]
        [RequireBotAdmin]
        public async Task GetCurrentMerchantIdAsync()
        {
            var merchantId = _merchantIdAccessor.GetMerchantId();
            
            await ReplyAndDeleteAsync($"Current configured merchant id: {merchantId}");
        }

        [Command("setcurrentmerchantid")]
        [Summary("Sets the configured merchant ID used by the bot.")]
        [RequireBotAdmin]
        public async Task SetCurrentMerchantIdAsync(string merchantId)
        {
            if (!Guid.TryParse(merchantId, out var parsedMerchantId))
            {
                throw new UserFriendlyException("Could not parse merchant id from the given input.");
            }

            await _merchantIdAccessor.SetMerchantId(parsedMerchantId);

            await ReplyAndDeleteAsync($"Set current configured merchant id to '{parsedMerchantId}'.");
        }

        private async Task<Embed> GetPluginEmbedAsync(ProductDto productDto)
        {
            var config = GetApiConfig();

            var productsApi = new ProductsApi(config);

            var productDetails = await productsApi.GetProductAsync(productDto.Id, ContentType.Markdown);

            var icon = productDto.LogoFiles.FirstOrDefault(x => x.Size == ProductLogoSize.Medium);

            var embedBuilder = new EmbedBuilder()
                .WithTitle(productDto.Name)
                .WithDescription($"Price: **{productDto.UnitPrice:C} USD** | " +
                                 $"Branches: **{string.Join(',', productDetails.Branches.Select(x => x.Name))}**")
                .WithUrl($"https://imperialplugins.com/Unturned/Products/{productDetails.NameId}")
                .WithAuthor(productDto.Merchant.Name)
                .WithTimestamp(productDto.CreationTime);

            if (icon != null)
            {
                embedBuilder.WithThumbnailUrl(icon.FileUrl);
            }

            var description = productDetails.Description;

            var descriptionLines = description.Split("\n").Select(x => x.TrimEnd()).ToArray();

            for (var i = 0; i < descriptionLines.Length; i++)
            {
                var header = $"**{descriptionLines[i].TrimStart('#').TrimStart()}**";

                var stringBuilder = new StringBuilder();

                for (i++; i < descriptionLines.Length; i++)
                {
                    if (descriptionLines[i].StartsWith('#'))
                    {
                        i--;
                        break;
                    }

                    if (stringBuilder.Length + descriptionLines[i].Length > 1024)
                    {
                        Log.Logger.Debug(header);
                        embedBuilder.AddFieldSafe(header, stringBuilder.ToString());
                        header = "\a";
                        stringBuilder = new StringBuilder();
                    }

                    stringBuilder.AppendLine(descriptionLines[i]);
                }

                embedBuilder.AddFieldSafe(header, stringBuilder.ToString());
            }

            return embedBuilder.Build();
        }

        [Command("createpluginchannels")]
        [Summary("Creates text channels for all plugins.")]
        [RequireBotAdmin]
        public async Task CreatePluginChannelsAsync(string categoryName)
        {
            var merchantId = _merchantIdAccessor.GetMerchantId();

            var config = GetApiConfig();

            // Category setup

            var category = (ICategoryChannel?)Context.Guild.CategoryChannels.FirstOrDefault(x =>
                               x.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase)) ??
                           await Context.Guild.CreateCategoryChannelAsync(categoryName);

            var everyoneRole = Context.Guild.EveryoneRole;

            await category.AddPermissionOverwriteAsync(everyoneRole,
                new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny, viewChannel: PermValue.Deny));

            // Get plugins

            var productsApi = new ProductsApi(config);

            var products =
                await productsApi.GetProductsAsync(merchantIds: new List<Guid> {merchantId}, maxResultCount: 100);

            // Enumerate text channels

            foreach (var productDto in products.Items)
            {
                var channelName = productDto.Name.Replace(' ', '-');

                var channel = (ITextChannel?) Context.Guild.Channels.FirstOrDefault(x =>
                                  x is SocketTextChannel textChannel
                                  && textChannel.CategoryId.HasValue
                                  && textChannel.CategoryId.Value == category.Id
                                  && x.Name.Equals(channelName,
                                      StringComparison.OrdinalIgnoreCase))
                              ?? await Context.Guild.CreateTextChannelAsync(channelName,
                                  properties => properties.CategoryId = category.Id);

                var pinnedMessages = await channel.GetPinnedMessagesAsync();

                var message =
                    pinnedMessages.FirstOrDefault(x => x.Author.Id == Context.Client.CurrentUser.Id) as IUserMessage;

                var embed = await GetPluginEmbedAsync(productDto);

                if (message == null)
                {
                    var newMessage = await channel.SendMessageAsync(embed: embed);

                    await newMessage.PinAsync();

                    var pinnedMessageNotification = await channel.GetMessagesAsync(10).Flatten().Where(x =>
                            x.Author.Id == Context.Client.CurrentUser.Id && x.Type == MessageType.ChannelPinnedMessage)
                        .FirstOrDefaultAsync();

                    if (pinnedMessageNotification != null)
                    {
                        await pinnedMessageNotification.DeleteAsync();
                    }
                }
                else
                {
                    await message.ModifyAsync(properties => properties.Embed = embed);
                }
            }
        }

        [Command("plugins")]
        [Summary("Get a list of plugins from Imperial Plugins belonging to the specified merchant.")]
        [RequireBotAdmin]
        public async Task GetPluginsAsync(string merchantName)
        {
            var config = GetApiConfig();

            var merchantsApi = new MerchantsApi();

            var merchants = (await merchantsApi.GetMerchantsAsync(maxResultCount: 200)).Items
                .Where(x => x.Name.Contains(merchantName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (merchants.Count == 0)
            {
                throw new UserFriendlyException("Merchant not found.");
            }

            if (merchants.Count > 1)
            {
                throw new UserFriendlyException($"Many possible merchants ({merchants.Count}).");
            }

            var merchant = merchants.Single();

            var productsApi = new ProductsApi(config);

            var products = await productsApi.GetProductsAsync(merchantIds: new List<Guid> {merchant.Id});

            var messages = new List<IMessage>();

            foreach (var productDto in products.Items)
            {
                var productDetails = await productsApi.GetProductAsync(productDto.Id, ContentType.Markdown);

                var icon = productDto.LogoFiles.FirstOrDefault(x => x.Size == ProductLogoSize.Medium);

                var embedBuilder = new EmbedBuilder()
                    .WithTitle(productDto.Name)
                    .WithDescription($"Price: **{productDto.UnitPrice:C} USD** | " +
                                     $"Branches: **{string.Join(',', productDetails.Branches.Select(x => x.Name))}**")
                    .WithUrl($"https://imperialplugins.com/Unturned/Products/{productDetails.NameId}")
                    .WithAuthor(merchant.Name)
                    .WithTimestamp(productDto.CreationTime);

                if (icon != null)
                {
                    embedBuilder.WithThumbnailUrl(icon.FileUrl);
                }

                var description = productDetails.Description;

                var descriptionLines = description.Split("\n").Select(x => x.TrimEnd()).ToArray();

                for (var i = 0; i < descriptionLines.Length; i++)
                {
                    var header = $"**{descriptionLines[i].TrimStart('#').TrimStart()}**";

                    var stringBuilder = new StringBuilder();

                    for (i++; i < descriptionLines.Length; i++)
                    {
                        if (descriptionLines[i].StartsWith('#'))
                        {
                            i--;
                            break;
                        }

                        if (stringBuilder.Length + descriptionLines[i].Length > 1024)
                        {
                            Log.Logger.Debug(header);
                            embedBuilder.AddFieldSafe(header, stringBuilder.ToString());
                            header = "\a";
                            stringBuilder = new StringBuilder();
                        }

                        stringBuilder.AppendLine(descriptionLines[i]);
                    }
                    
                    embedBuilder.AddFieldSafe(header, stringBuilder.ToString());
                }

                messages.Add(await ReplyAsync(embed: embedBuilder.Build()));
            }

            await Task.Delay(60000);

            foreach (var message in messages)
            {
                await message.DeleteAsync();
            }
        }
    }
}
