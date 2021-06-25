using Autofac;
using Microsoft.Extensions.DependencyInjection;
using SilKsPlugins.DiscordBot.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SilKsPlugins.DiscordBot.Components
{
    public class ComponentManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILifetimeScope _lifetimeScope;

        private readonly List<IComponent> _loadedComponents;

        public ComponentManager(IServiceProvider serviceProvider, ILifetimeScope lifetimeScope)
        {
            _serviceProvider = serviceProvider;
            _lifetimeScope = lifetimeScope;

            _loadedComponents = ConstructComponents();
        }

        private List<IComponent> ConstructComponents()
        {
            var components = new List<IComponent>();

            var componentTypes = GetType().Assembly.FindTypes<IComponent>();

            foreach (var componentType in componentTypes)
            {
                var component = (IComponent)ActivatorUtilities.CreateInstance(_serviceProvider, componentType);

                if (component is IAsyncDisposable asyncDisposable)
                {
                    _lifetimeScope.Disposer.AddInstanceForAsyncDisposal(asyncDisposable);
                }
                else if (component is IDisposable disposable)
                {
                    _lifetimeScope.Disposer.AddInstanceForDisposal(disposable);
                }

                components.Add(component);
            }

            return components;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var component in _loadedComponents)
            {
                await component.StartAsync(cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var component in _loadedComponents)
            {
                await component.StopAsync(cancellationToken);
            }
        }
    }
}
