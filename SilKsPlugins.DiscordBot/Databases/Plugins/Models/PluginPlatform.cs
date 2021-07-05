using System;

namespace SilKsPlugins.DiscordBot.Databases.Plugins.Models
{
    [Flags]
    public enum PluginPlatform
    {
        None = 0,
        OpenMod = 1,
        RocketMod = 2,
        All = OpenMod | RocketMod
    }
}
