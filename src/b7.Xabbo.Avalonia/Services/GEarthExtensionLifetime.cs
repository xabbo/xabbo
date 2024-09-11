using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Avalonia.Controls.ApplicationLifetimes;

using Xabbo.GEarth;

namespace b7.Xabbo.Avalonia.Services;

public class GEarthExtensionLifetime
{
    private readonly IControlledApplicationLifetime _lifetime;
    private readonly GEarthExtension _ext;

    public GEarthExtensionLifetime(
        IControlledApplicationLifetime appLifetime,
        GEarthExtension extension)
    {
        // ArgumentNullException.ThrowIfNull(appLifetime);
        ArgumentNullException.ThrowIfNull(extension);

        _lifetime = appLifetime;
        _ext = extension;
    }

    public async Task RunAsync()
    {
        try
        {
            await _ext.RunAsync();
        }
        finally
        {
            OnInterceptorDisconnected();
        }
    }

    private void OnInterceptorDisconnected()
    {
        try
        {
            _lifetime.Shutdown();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
}
