using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Humanizer;
using Microsoft.Extensions.Hosting;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Messages.Incoming;

using Xabbo.Extension;
using Xabbo.Services.Abstractions;

namespace Xabbo.Avalonia.Services;

public class AppSessionManager
{
    private readonly Application _application;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IUiContext _uiContext;
    private readonly IExtension _extension;
    private readonly IGameDataManager _gameDataManager;
    private readonly ProfileManager _profileManager;

    private DisconnectReason _currentDisconnectReason = DisconnectReason.Unknown;

    public AppSessionManager(Application application,
        IHostApplicationLifetime lifetime,
        IUiContext uiContext,
        IExtension extension,
        IGameDataManager gameDataManager,
        ProfileManager profileManager)
    {
        _application = application;
        _lifetime = lifetime;
        _uiContext = uiContext;
        _extension = extension;
        _gameDataManager = gameDataManager;
        _profileManager = profileManager;

        _extension.Connected += OnConnected;
        _extension.Disconnected += OnDisconnected;

        lifetime.ApplicationStopping.Register(OnStopping);
    }


    private void SetStatus(string status)
    {
        _uiContext.Invoke(() => _application.Resources["AppStatus"] = status);
    }

    private void OnConnected(ConnectedEventArgs e)
    {
        _currentDisconnectReason = DisconnectReason.Unknown;
        _extension.Intercept<DisconnectReasonMsg>(HandleDisconnectReason);

        _uiContext.Invoke(() =>
        {
            _application.Resources["IsConnecting"] = false;
            _application.Resources["IsConnected"] = true;
            _application.Resources["IsOrigins"] = e.Session.Is(ClientType.Origins);
            _application.Resources["IsModern"] = !e.Session.Is(ClientType.Origins);
        });

        CancellationToken ct = _extension.DisconnectToken;
        Task.Run(() => InitializeAsync(e.Session, ct));
    }

    private async Task InitializeAsync(Session session, CancellationToken ct)
    {
        try
        {
            SetStatus($"Loading game data for {session.Hotel.Name} hotel...");
            await _gameDataManager.WaitForLoadAsync(ct);

            SetStatus($"Waiting for user data...");
            await _profileManager.GetUserDataAsync();

            _uiContext.Invoke(() =>
            {
                _application.Resources["IsReady"] = true;
            });
        }
        catch { }
    }

    void HandleDisconnectReason(DisconnectReasonMsg msg)
    {
        _currentDisconnectReason = msg.Reason;
    }

    private void OnDisconnected()
    {
        _uiContext.Invoke(() =>
        {
            _application.Resources["AppStatus"] = _currentDisconnectReason switch
            {
                DisconnectReason.Unknown or
                DisconnectReason.Disconnected => "Disconnected!\nWaiting for another connection...",
                _ => $"Disconnected! {_currentDisconnectReason.Humanize()}.\nWaiting for another connection..."
            };
            _application.Resources["IsConnecting"] = true;
            _application.Resources["IsConnected"] = false;
            _application.Resources["IsReady"] = false;
        });
    }

    private void OnStopping()
    {
        _uiContext.Invoke(() =>
        {
            _application.Resources["IsConnecting"] = false;
            _application.Resources["IsConnected"] = false;
            _application.Resources["AppStatus"] = "Connection to G-Earth has ended.\nxabbo will now shut down.";
        });

        // Delay shutdown for visual effect.
        Task.Delay(5000).GetAwaiter().GetResult();
    }
}