using System.ComponentModel.DataAnnotations;

namespace SilKsPlugins.DiscordBot.Databases.Administration.Models
{
    public class LogChannel
    {
        public ulong GuildId { get; set; }
        
        public ulong ChannelId { get; set; }
    }
}
