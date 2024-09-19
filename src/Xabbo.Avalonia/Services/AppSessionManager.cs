using Avalonia;

using Xabbo.Extension;
using Xabbo.Services.Abstractions;

namespace Xabbo.Avalonia.Services;

public class AppSessionManager
{
    private readonly Application _application;
    private readonly IUiContext _uiContext;
    private readonly IExtension _extension;

    public AppSessionManager(Application application, IUiContext uiContext, IExtension extension)
    {
        _application = application;
        _uiContext = uiContext;
        _extension = extension;

        _extension.Connected += OnConnected;
        _extension.Disconnected += OnDisconnected;
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
}