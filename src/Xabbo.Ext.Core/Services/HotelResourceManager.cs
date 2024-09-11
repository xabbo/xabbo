using Microsoft.Extensions.Hosting;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Core.GameData;

namespace Xabbo.Ext.Services;

/// <summary>
/// Manages resources for the local hotel.
/// </summary>
public class HotelResourceManager : IHostedService
{
    private readonly IUriProvider<HabboEndpoints> _uriProvider;
    private readonly IGameDataManager _gameDataManager;

    private CancellationTokenSource? _ctsLoad;

    public HotelResourceManager(
        IExtension extension,
        IUriProvider<HabboEndpoints> uriProvider,
        IGameDataManager gameDataManager)
    {
        extension.Connected += OnGameConnected;

        _uriProvider = uriProvider;
        _gameDataManager = gameDataManager;

        _gameDataManager.Loaded += OnGameDataLoaded;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void OnGameConnected(GameConnectedArgs e)
    {
        _ctsLoad?.Cancel();
        _ctsLoad = new CancellationTokenSource();
        CancellationToken ct = _ctsLoad.Token;

        try
        {
            Hotel hotel = Hotel.FromGameHost(e.Host);
            _uriProvider.Host = hotel.HostName;
            Task.Run(() => _gameDataManager.LoadAsync(hotel, ct));
        }
        catch (Exception)
        {
            // Failed to find hotel
        }
    }

    private void OnGameDataLoaded()
    {
        FurniData? furni = _gameDataManager.Furni;
        ExternalTexts? texts = _gameDataManager.Texts;
    }
}
