using SilKsPlugins.DiscordBot.Database;
using System;

namespace SilKsPlugins.DiscordBot.Discord.RoleReactions
{
    public class RoleMessagesDbContext : MySqlDbContext
    {
        protected RoleMessagesDbContext(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
