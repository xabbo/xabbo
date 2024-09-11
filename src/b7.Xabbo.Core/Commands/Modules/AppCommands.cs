using b7.Xabbo.Core.Services;

namespace b7.Xabbo.Commands;

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
