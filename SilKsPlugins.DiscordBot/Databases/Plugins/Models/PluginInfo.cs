using System;

namespace SilKsPlugins.DiscordBot.Databases.Plugins.Models
{
    public class PluginInfo
    {
        public string Id { get; set; } = "";

        public string Title { get; set; } = "";

        public string? Description { get; set; }

        public string? Url { get; set; }

        public string? IconUrl { get; set; }

        public decimal? Price { get; set; }

        public ulong? ChannelId { get; set; }

        public string? Author { get; set; }

        public DateTimeOffset? CreationTime { get; set; }

        public PluginPlatform Platforms { get; set; }

        public string? Content { get; set; }

        public string CategoryId { get; set; } = "";

        public CategoryInfo Category { get; set; } = null!;
    }
}
