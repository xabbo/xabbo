using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Avalonia.Controls.ApplicationLifetimes;

namespace Xabbo.Avalonia.Services;

using IHostApplicationLifetime = Microsoft.Extensions.Hosting.IHostApplicationLifetime;

public sealed class AvaloniaHostApplicationLifetime : IHostApplicationLifetime
{
    private readonly ILogger _logger;
    private readonly IControlledApplicationLifetime _appLifetime;

    private readonly CancellationTokenSource
        _started = new(),
        _stopping = new(),
        _stopped = new();

    public CancellationToken ApplicationStarted => _started.Token;
    public CancellationToken ApplicationStopping => _stopping.Token;
    public CancellationToken ApplicationStopped => _stopped.Token;

    public AvaloniaHostApplicationLifetime(
        ILoggerFactory loggerFactory,
        IApplicationLifetime appLifetime)
    {
        _logger = loggerFactory.CreateLogger<AvaloniaHostApplicationLifetime>();

        if (appLifetime is not IControlledApplicationLifetime controlledLifetime)
            throw new NotSupportedException("Application lifetime is not controlled.");

        _appLifetime = controlledLifetime;
        _appLifetime.Startup += OnStartup;
        _appLifetime.Exit += OnExit;
    }

    private void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        _logger.LogInformation("Starting application...");

        _started.Cancel();

        _logger.LogInformation("Application started.");
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _logger.LogInformation("OnExit");
    }

    public void StopApplication()
    {
        if (_stopping.IsCancellationRequested)
            return;

        try
        {
            _logger.LogInformation("Shutting down...");
            _stopping.Cancel();

            _appLifetime.Shutdown();

            // TODO: application exits before we get here.
            // Exit event is also not invoked for some reason.
            _stopped.Cancel();
            _logger.LogInformation("Shutdown complete.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error shutting down.");
        }
    }
}