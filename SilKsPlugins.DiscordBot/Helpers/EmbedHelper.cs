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

        public static EmbedBuilder AddFieldSafe(this EmbedBuilder embedBuilder, string name, string value, bool inline = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "\a";
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                value = "\a";
            }

            return embedBuilder.AddField(name, value, inline);
        }
    }
}
