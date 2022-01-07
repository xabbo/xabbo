using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Xabbo.Interceptor;
using Xabbo.Core;
using Xabbo.Core.GameData;

namespace b7.Xabbo.Services
{
    /// <summary>
    /// Manages resources for the local hotel.
    /// </summary>
    public class HotelResourceManager : IHostedService
    {
        private readonly IDictionary<string, string> _domainMap;
        private readonly IUriProvider<HabboEndpoints> _uriProvider;
        private readonly IGameDataManager _gameDataManager;

        private CancellationTokenSource? _ctsLoad;

        public HotelResourceManager(
            IConfiguration config,
            IInterceptor interceptor,
            IUriProvider<HabboEndpoints> uriProvider,
            IGameDataManager gameDataManager)
        {
            _domainMap = new Dictionary<string, string>();
            config.Bind("DomainMap", _domainMap);

            interceptor.Connected += OnGameConnected;

            _uriProvider = uriProvider;
            _gameDataManager = gameDataManager;

            _gameDataManager.Loaded += OnGameDataLoaded;
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private void OnGameConnected(object? sender, GameConnectedEventArgs e)
        {
            _ctsLoad?.Cancel();
            _ctsLoad = new CancellationTokenSource();
            CancellationToken ct = _ctsLoad.Token;

            Match m = Regex.Match(e.Host, @"^game-(?<host>[a-z]+)\.habbo\.com$");
            if (!m.Success) return;

            string hostIdentifier = m.Groups[1].Value;

            if (!_domainMap.TryGetValue(hostIdentifier, out string? domain))
                domain = hostIdentifier;

            _uriProvider.Domain = domain;

            Task.Run(() => _gameDataManager.LoadAsync(domain, ct));
        }

        private void OnGameDataLoaded()
        {
            FurniData? furni = _gameDataManager.Furni;
            ExternalTexts? texts = _gameDataManager.Texts;

            if (furni is null || texts is null) return;

            XabboCoreExtensions.Initialize(furni, texts);
        }
    }
}
