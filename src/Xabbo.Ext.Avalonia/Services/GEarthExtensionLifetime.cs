using System;
using System.Threading.Tasks;

using Avalonia.Controls.ApplicationLifetimes;

using Xabbo.GEarth;
using Xabbo.Ext.Core.Services;

namespace Xabbo.Ext.Avalonia.Services;

public class GEarthExtensionLifetime
{
    private readonly IApplicationLifetime _lifetime;
    private readonly IApplicationManager _app;
    private readonly GEarthExtension _ext;

    public GEarthExtensionLifetime(
        IApplicationLifetime appLifetime,
        IApplicationManager appManager,
        GEarthExtension extension)
    {
        ArgumentNullException.ThrowIfNull(appLifetime);
        ArgumentNullException.ThrowIfNull(appManager);
        ArgumentNullException.ThrowIfNull(extension);

        _lifetime = appLifetime;
        _app = appManager;
        _ext = extension;

        _ext.Activated += OnActivated;
    }

    private void OnActivated() => _app.BringToFront();

    public async Task RunAsync()
    {
        try
        {
            await _ext.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            if (_lifetime is IControlledApplicationLifetime lifetime)
                lifetime.Shutdown();
        }
    }
}
