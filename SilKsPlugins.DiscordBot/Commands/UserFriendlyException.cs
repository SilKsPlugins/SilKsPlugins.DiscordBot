using System;

namespace SilKsPlugins.DiscordBot.Commands
{
    public class UserFriendlyException : Exception
    {
        public UserFriendlyException()
        {
        }

        public UserFriendlyException(string message) : base(message)
        {
        }
    }
}
