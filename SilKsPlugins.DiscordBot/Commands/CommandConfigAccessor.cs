using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SilKsPlugins.DiscordBot.Commands
{
    public class CommandConfigAccessor
    {
        private readonly IConfiguration _configuration;

        public CommandConfigAccessor(IConfiguration configuration)
        {
            _configuration = configuration.GetSection("Commands");
        }

        public string CommandPrefix => _configuration["Prefix"];

        public int DeleteStandardReplyDelay =>
            (int) (_configuration.GetValue("Prefix:DeleteReplyDelay:Standard", 15f) * 1000);

        public int DeleteErrorReplyDelay =>
            (int) (_configuration.GetValue("Prefix:DeleteReplyDelay:Error", 15f) * 1000);
    }
}
