using System.Collections.Generic;
using System.Linq;

namespace SilKsPlugins.DiscordBot.Logging.Configuration
{
    public class DiscordChannelLogConfigurer : IDiscordChannelLogConfigurer
    {
        private readonly HashSet<ulong> _channelIds;

        public DiscordChannelLogConfigurer()
        {
            _channelIds = new HashSet<ulong>();
        }

        public IReadOnlyCollection<ulong> GetChannelIds()
        {
            lock (_channelIds)
            {
                return _channelIds.ToList();
            }
        }

        public bool AddChannel(ulong channelId)
        {
            lock (_channelIds)
            {
                return _channelIds.Add(channelId);
            }
        }

        public bool RemoveChannel(ulong channelId)
        {
            lock (_channelIds)
            {
                return _channelIds.Remove(channelId);
            }
        }
    }
}
