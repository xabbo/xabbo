using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Microsoft.Extensions.Hosting;

using Xabbo.Extension;
using Xabbo.Services.Abstractions;

namespace Xabbo.Avalonia.Services;

public class AppSessionManager
{
    private readonly Application _application;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IUiContext _uiContext;
    private readonly IExtension _extension;

    public AppSessionManager(Application application,
        IHostApplicationLifetime lifetime,
        IUiContext uiContext,
        IExtension extension)
    {
        _application = application;
        _lifetime = lifetime;
        _uiContext = uiContext;
        _extension = extension;

        _extension.Connected += OnConnected;
        _extension.Disconnected += OnDisconnected;

        lifetime.ApplicationStopping.Register(OnStopping);
    }

    private void OnConnected(GameConnectedArgs e)
    {
        _uiContext.Invoke(() =>
        {
            _application.Resources["IsConnecting"] = false;
            _application.Resources["IsConnected"] = true;
            _application.Resources["IsOrigins"] = e.Session.IsShockwave;
            _application.Resources["IsModern"] = !e.Session.IsShockwave;
        });
    }

    private void OnDisconnected()
    {
        _uiContext.Invoke(() =>
        {
            _application.Resources["IsConnecting"] = true;
            _application.Resources["IsConnected"] = false;
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
        Task.Delay(6000).GetAwaiter().GetResult();
    }
}