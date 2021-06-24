using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using System;

namespace SilKsPlugins.DiscordBot.Logging
{
    public static class DiscordSinkExtensions
    {
        public static LoggerConfiguration DiscordSink(
            this LoggerSinkConfiguration loggerConfiguration,
            IServiceProvider serviceProvider,
            IFormatProvider? formatProvider = null)
        {
            var sink = formatProvider == null
                ? ActivatorUtilities.CreateInstance<DiscordSink>(serviceProvider)
                : ActivatorUtilities.CreateInstance<DiscordSink>(serviceProvider, formatProvider);

            return loggerConfiguration.Sink(sink);
        }
    }
}
