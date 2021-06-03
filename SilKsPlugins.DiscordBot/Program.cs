﻿using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot
{
    public class Program
    {
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var runtime = new Runtime();
            
            await runtime.InitAsync();
        }
    }
}
