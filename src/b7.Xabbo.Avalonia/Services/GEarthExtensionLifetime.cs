using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        _ext.InterceptorDisconnected += OnInterceptorDisconnected;
        await _ext.RunAsync();
    }

    private void OnInterceptorDisconnected(object? sender, global::Xabbo.Extension.DisconnectedEventArgs e)
    {
        try
        {

        _lifetime.Shutdown();
        } catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
}
