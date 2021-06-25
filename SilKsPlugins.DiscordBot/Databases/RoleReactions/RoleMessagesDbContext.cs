using SilKsPlugins.DiscordBot.MySql;
using System;

namespace SilKsPlugins.DiscordBot.Databases.RoleReactions
{
    public class RoleMessagesDbContext : MySqlDbContext
    {
        protected RoleMessagesDbContext(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
