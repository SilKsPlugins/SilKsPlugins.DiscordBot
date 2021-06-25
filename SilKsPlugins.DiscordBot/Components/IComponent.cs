using System.Threading;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Components
{
    public interface IComponent
    {
        Task StartAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}
