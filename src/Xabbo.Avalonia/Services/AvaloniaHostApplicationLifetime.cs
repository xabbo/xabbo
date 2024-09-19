using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Avalonia.Controls.ApplicationLifetimes;

namespace Xabbo.Avalonia.Services;

using IHostApplicationLifetime = Microsoft.Extensions.Hosting.IHostApplicationLifetime;

public sealed class AvaloniaHostApplicationLifetime : IHostApplicationLifetime
{
    private readonly ILogger Log;
    private readonly IApplicationLifetime _lifetime;

    private readonly CancellationTokenSource
        _started = new(),
        _stopping = new(),
        _stopped = new();

    public CancellationToken ApplicationStarted => _started.Token;
    public CancellationToken ApplicationStopping => _stopping.Token;
    public CancellationToken ApplicationStopped => _stopped.Token;

    public AvaloniaHostApplicationLifetime(
        ILoggerFactory loggerFactory,
        IApplicationLifetime lifetime)
    {
        Log = loggerFactory.CreateLogger<AvaloniaHostApplicationLifetime>();

        _lifetime = lifetime;

        if (lifetime is IControlledApplicationLifetime controlledLifetime)
        {
            controlledLifetime.Startup += OnStartup;
            controlledLifetime.Exit += OnExit;
        }
        else
        {
            throw new Exception("Application lifetime is not controlled.");
        }
    }

    private void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        Log.LogInformation("Starting application...");
        _started.Cancel();
        Log.LogInformation("Application started.");
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Log.LogInformation("OnExit");
    }

    public void StopApplication()
    {
        try
        {
            if (_lifetime is IControlledApplicationLifetime controlledLifetime)
            {
                Log.LogInformation("Shutting down...");
                _stopping.Cancel();
                controlledLifetime.Shutdown();
                // TODO: application exits before we get here.
                // Exit event is also not invoked for some reason.
                _stopped.Cancel();
                Log.LogInformation("Shutdown complete.");
            }
            else
            {
                Log.LogWarning("Cannot stop application: lifetime is not controlled.");
            }

        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Error shutting down.");
        }
    }
}