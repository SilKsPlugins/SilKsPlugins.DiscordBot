using System.Collections.Generic;

namespace SilKsPlugins.DiscordBot.Logging.Configuration
{
    public interface IDiscordChannelLogConfigurer
    {
        IReadOnlyCollection<ulong> GetChannelIds();
        
        bool AddChannel(ulong channelId);
        bool RemoveChannel(ulong channelId);
    }
}
