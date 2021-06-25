using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot
{
    public class Program
    {
        public static async Task Main()
        {
            var runtime = new Runtime();

            await runtime.InitAsync();
        }
    }
}
