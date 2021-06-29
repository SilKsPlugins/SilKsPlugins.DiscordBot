using SilKsPlugins.DiscordBot.Discord.Commands;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Discord.Modules.Plugins.Services
{
    public class MerchantIdAccessor
    {
        private const string MerchantIdFile = "merchantid.dat";

        private readonly Runtime _runtime;

        private Guid? _merchantId;

        public MerchantIdAccessor(Runtime runtime)
        {
            _runtime = runtime;

            Task.Run(async () =>
            {
                var path = GetMerchantIdPath();

                if (File.Exists(path))
                {
                    var fileContents = await File.ReadAllTextAsync(path);

                    if (Guid.TryParse(fileContents, out var merchantId))
                    {
                        _merchantId = merchantId;
                    }
                }
            });
        }

        private string GetMerchantIdPath()
        {
            return Path.Combine(_runtime.WorkingDirectory, MerchantIdFile);
        }

        public Guid GetMerchantId() =>
            GetMerchantIdNullable() ?? throw new UserFriendlyException("No merchant ID configured.");

        public Guid? GetMerchantIdNullable()
        {
            return _merchantId;
        }

        public async Task SetMerchantId(Guid merchantId)
        {
            var path = GetMerchantIdPath();

            await File.WriteAllTextAsync(path, merchantId.ToString());

            _merchantId = merchantId;
        }
    }
}
