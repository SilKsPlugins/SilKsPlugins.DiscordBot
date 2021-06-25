﻿using Discord;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.RoleReactions.Services
{
    public interface IRoleReactionMessageManager
    {
        Task AddReactionToMessage(IMessage message);

        Task RemoveReactionFromMessage(IMessage message);
    }
}