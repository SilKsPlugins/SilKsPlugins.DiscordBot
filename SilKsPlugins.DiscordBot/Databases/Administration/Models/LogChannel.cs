using System.ComponentModel.DataAnnotations;

namespace SilKsPlugins.DiscordBot.Databases.Administration.Models
{
    public class LogChannel
    {
        [Key]
        public ulong ChannelId { get; set; }
    }
}
