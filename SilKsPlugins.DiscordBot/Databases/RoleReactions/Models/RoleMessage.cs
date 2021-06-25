namespace SilKsPlugins.DiscordBot.Databases.RoleReactions.Models
{
    public class RoleMessage
    {
        public ulong GuildId { get; set; }

        public ulong ChannelId { get; set; }

        public ulong MessageId { get; set; }

        public ulong RoleId { get; set; }
    }
}
