using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using b7.Xabbo.Components;

namespace b7.Xabbo.Services
{
    public class InitializationService : IHostedService
    {
        private readonly IEnumerable<Component> _components;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IUiContext _uiContext;
        private readonly IGameDataManager _gameDataManager;

        public InitializationService(
            IHostApplicationLifetime lifetime,
            IUiContext uiContext,
            IGameDataManager gameDataManager,
            IEnumerable<Component> components)
        {
            _lifetime = lifetime;
            _uiContext = uiContext;
            _gameDataManager = gameDataManager;
            _components = components;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _gameDataManager.InitializeAsync();
            }
            catch (Exception ex)
            {
                await _uiContext.InvokeAsync(() => Dialog.ShowError($"An error occurred during initialization.\r\n{ex}"));
                _lifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
