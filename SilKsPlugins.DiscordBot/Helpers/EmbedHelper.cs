using Discord;

namespace SilKsPlugins.DiscordBot.Helpers
{
    public static class EmbedHelper
    {
        public static Embed SimpleEmbed(string message, Color? color = null)
        {
            var builder = new EmbedBuilder()
                .WithTitle(message);

            if (color != null)
            {
                builder.WithColor(color.Value);
            }

            return builder.Build();
        }
    }
}
