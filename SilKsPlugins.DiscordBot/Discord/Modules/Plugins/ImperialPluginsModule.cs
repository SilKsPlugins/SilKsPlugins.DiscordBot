using Discord;
using Discord.Commands;
using ImperialPlugins.Tools.Api;
using ImperialPlugins.Tools.Client;
using ImperialPlugins.Tools.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Serilog;
using SilKsPlugins.DiscordBot.Databases.Plugins.Models;
using SilKsPlugins.DiscordBot.Discord.Commands;
using SilKsPlugins.DiscordBot.Discord.Modules.Plugins.Services;
using SilKsPlugins.DiscordBot.Discord.Preconditions;
using SilKsPlugins.DiscordBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Modules.Plugins
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [RequireBotAdmin]
    public class ImperialPluginsModule : CustomModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;
        private readonly MerchantIdAccessor _merchantIdAccessor;
        private readonly PluginManager _pluginManager;

        public ImperialPluginsModule(IConfiguration configuration,
            MerchantIdAccessor merchantIdAccessor,
            PluginManager pluginManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configuration = configuration;
            _merchantIdAccessor = merchantIdAccessor;
            _pluginManager = pluginManager;
        }

        private Configuration GetApiConfig()
        {
            var config = new Configuration();

            config.ApiKey.Add("X-API-KEY", _configuration["ImperialPluginsApiKey"]);

            return config;
        }

        [Command("getmerchantid")]
        [Summary("Gets the given merchant's ID.")]
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
        public async Task GetCurrentMerchantIdAsync()
        {
            var merchantId = _merchantIdAccessor.GetMerchantId();
            
            await ReplyAndDeleteAsync($"Current configured merchant id: {merchantId}");
        }

        [Command("setcurrentmerchantid")]
        [Summary("Sets the configured merchant ID used by the bot.")]
        public async Task SetCurrentMerchantIdAsync(string merchantId)
        {
            if (!Guid.TryParse(merchantId, out var parsedMerchantId))
            {
                throw new UserFriendlyException("Could not parse merchant id from the given input.");
            }

            await _merchantIdAccessor.SetMerchantId(parsedMerchantId);

            await ReplyAndDeleteAsync($"Set current configured merchant id to '{parsedMerchantId}'.");
        }

        [Command("ip plugins import")]
        [Summary("Imports plugins from Imperial Plugins to the bot's database.")]
        public async Task PluginsImportAsync(string categoryId)
        {
            var merchantId = _merchantIdAccessor.GetMerchantId();

            var config = GetApiConfig();

            var category = await _pluginManager.GetCategory(categoryId) ??
                           throw new UserFriendlyException("The specified category could not be found.");

            var productsApi = new ProductsApi(config);

            var products = await productsApi.GetProductsAsync(merchantIds: new List<Guid> { merchantId }, isPublic: true,
                maxResultCount: 100);

            void UpdatePluginInfo(PluginInfo plugin, ProductWithDetailsDto productWithDetails)
            {
                plugin.Title = productWithDetails.Name;
                plugin.Description = productWithDetails.ShortDescription;
                plugin.Url = $"https://imperialplugins.com/Unturned/Products/{productWithDetails.NameId}";
                plugin.IconUrl =
                    productWithDetails.LogoFiles.FirstOrDefault(x => x.Size == ProductLogoSize.Medium)?.FileUrl;
                plugin.Price = productWithDetails.UnitPrice == 0 ? null : (decimal?)productWithDetails.UnitPrice;
                plugin.Author = productWithDetails.Merchant.Name;
                plugin.CreationTime = productWithDetails.CreationTime;

                plugin.Platforms = PluginPlatform.None;

                if (productWithDetails.Branches.Any(x =>
                    x.Identifier.Equals("openmod", StringComparison.OrdinalIgnoreCase)))
                {
                    plugin.Platforms |= PluginPlatform.OpenMod;
                }

                if (productWithDetails.Branches.Any(x =>
                    x.Identifier.Equals("rocketmod", StringComparison.OrdinalIgnoreCase)))
                {
                    plugin.Platforms |= PluginPlatform.RocketMod;
                }

                plugin.Content =
                    Regex.Replace(productWithDetails.Description, @"$\#+\s*", "# ", RegexOptions.Multiline);
                plugin.Category = category;
            }

            var output = "**Imported Plugins:**";

            foreach (var product in products.Items)
            {
                var productWithDetails = await productsApi.GetProductAsync(product.Id, ContentType.Markdown);

                var pluginId = product.GameCategory.CategoryName.Replace(' ', '-').ToLower() + "-" +
                               product.NameId.Replace(' ', '-').ToLower();

                var plugin = await _pluginManager.GetPlugin(pluginId);

                if (plugin != null)
                {
                    if (await _pluginManager.UpdatePlugin(plugin.Id, x => UpdatePluginInfo(x, productWithDetails)))
                    {
                        output += $"\nUpdate {plugin.Id} plugin.";
                    }
                    else
                    {
                        output += $"\n**ERROR:** Could not update {plugin.Id} plugin.";
                    }
                }
                else
                {
                    plugin = new PluginInfo
                    {
                        Id = plugin?.Id ?? pluginId
                    };

                    UpdatePluginInfo(plugin, productWithDetails);

                    if (await _pluginManager.AddPlugin(plugin))
                    {
                        output += $"\nAdded {plugin.Id} plugin.";
                    }
                    else
                    {
                        output += $"\n**ERROR:** Could not add {plugin.Id} plugin.";
                    }
                }
            }

            await ReplyAsync(output);
        }

        [Command("ip plugins")]
        [Summary("Get a list of plugins from Imperial Plugins belonging to the specified merchant.")]
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
