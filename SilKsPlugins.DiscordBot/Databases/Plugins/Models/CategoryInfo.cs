using System.Collections.Generic;

namespace SilKsPlugins.DiscordBot.Databases.Plugins.Models
{
    public class CategoryInfo
    {
        public string Id { get; set; } = "";

        public string Title { get; set; } = "";

        public ulong? ChannelCategoryId { get; set; }

        public ICollection<PluginInfo> Plugins { get; set; } = new List<PluginInfo>();
    }
}
