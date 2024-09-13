using Avalonia;

using Xabbo.Extension;

namespace Xabbo.Ext.Avalonia.Services;

public class AppSessionManager
{
    private readonly Application _application;
    private readonly IExtension _extension;

    public AppSessionManager(Application application, IExtension extension)
    {
        _application = application;
        _extension = extension;

        _extension.Connected += OnConnected;
        _extension.Disconnected += OnDisconnected;
    }

    private void OnConnected(GameConnectedArgs e)
    {
        _application.Resources["IsConnecting"] = false;
        _application.Resources["IsConnected"] = true;
        _application.Resources["IsOrigins"] = e.Session.IsShockwave;
        _application.Resources["IsModern"] = !e.Session.IsShockwave;
    }

    private void OnDisconnected()
    {
        _application.Resources["IsConnecting"] = true;
        _application.Resources["IsConnected"] = false;
    }
}