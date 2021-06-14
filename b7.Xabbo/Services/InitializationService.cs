using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace b7.Xabbo.Services
{
    public class InitializationService : IHostedService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IUiContext _uiContext;
        private readonly IGameDataManager _gameDataManager;

        public InitializationService(
            IHostApplicationLifetime lifetime,
            IUiContext uiContext,
            IGameDataManager gameDataManager)
        {
            _lifetime = lifetime;
            _uiContext = uiContext;
            _gameDataManager = gameDataManager;
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
