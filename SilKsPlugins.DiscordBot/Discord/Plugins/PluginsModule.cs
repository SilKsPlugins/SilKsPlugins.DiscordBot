using Discord;
using Discord.Commands;
using JetBrains.Annotations;
using SilKsPlugins.DiscordBot.Discord.Preconditions;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Plugins
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PluginsModule : ModuleBase<SocketCommandContext>
    {
        [Command("sayembed")]
        [Summary("Responds with an embed and deletes the original message.")]
        [RequireBotAdmin]
        public async Task SayEmbedAsync()
        {
            var embedBuilder = new EmbedBuilder();

            var embed = embedBuilder.WithTitle("Test title")
                .WithAuthor("Test author")
                .WithDescription("Test description")
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
