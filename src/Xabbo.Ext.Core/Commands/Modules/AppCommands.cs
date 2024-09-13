using Xabbo.Ext.Core.Services;

namespace Xabbo.Ext.Commands;

[CommandModule]
public sealed class AppCommands(IApplicationManager app) : CommandModule
{
    private readonly IApplicationManager _app = app;

    [Command("x")]
    public Task OnShowWindow(CommandArgs args)
    {
        _app.BringToFront();

        return Task.CompletedTask;
    }
}
