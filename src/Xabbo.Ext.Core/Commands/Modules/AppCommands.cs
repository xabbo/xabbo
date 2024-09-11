using Xabbo.Ext.Core.Services;

namespace Xabbo.Ext.Commands;

public class AppCommands(IApplicationManager app) : CommandModule
{
    private readonly IApplicationManager _app = app;

    [Command("x")]
    public Task OnShowWindow()
    {
        _app.BringToFront();

        return Task.CompletedTask;
    }
}
