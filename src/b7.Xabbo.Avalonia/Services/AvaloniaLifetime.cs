using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Controls;

namespace b7.Xabbo.Avalonia.Services;

internal class AvaloniaLifetime : IHostLifetime
{
    private readonly App _app;
    private readonly AppBuilder _appBuilder;

    public AvaloniaLifetime(AppBuilder appBuilder)
    {
        _appBuilder = appBuilder;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Debug.WriteLine("StopAsync");
        return Task.CompletedTask;
    }

    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        Debug.WriteLine("WaitForStartAsync");

        // app.StartWithClassicDesktopLifetime(Environment.GetCommandLineArgs());
        _appBuilder.Start((app, args) =>
        {
            app.Run(CancellationToken.None);
        }, Environment.GetCommandLineArgs());

        Debug.WriteLine("WaitForStartAsyncEnd");
        return Task.CompletedTask;
    }
}
