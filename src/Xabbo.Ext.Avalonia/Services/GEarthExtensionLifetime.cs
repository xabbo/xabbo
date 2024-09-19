using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xabbo.GEarth;
using Xabbo.Ext.Core.Services;

namespace Xabbo.Ext.Avalonia.Services;

public class GEarthExtensionLifetime
{
    private readonly ILogger Log;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IApplicationManager _app;
    private readonly GEarthExtension _ext;

    public GEarthExtensionLifetime(
        ILoggerFactory loggerFactory,
        IHostApplicationLifetime hostLifetime,
        IApplicationManager appManager,
        GEarthExtension extension)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(hostLifetime);
        ArgumentNullException.ThrowIfNull(appManager);
        ArgumentNullException.ThrowIfNull(extension);

        Log = loggerFactory.CreateLogger<GEarthExtensionLifetime>();
        _lifetime = hostLifetime;
        _app = appManager;
        _ext = extension;

        _lifetime.ApplicationStarted.Register(OnApplicationStarted);

        _ext.Activated += OnActivated;
    }

    private void OnApplicationStarted() => Task.Run(RunAsync);

    private void OnActivated() => _app.BringToFront();

    public async Task RunAsync()
    {
        try
        {
            Log.LogInformation("Running G-Earth extension.");
            await _ext.RunAsync();
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Exception occurred in G-Earth extension handler.");
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }
}
