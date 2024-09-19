using Xabbo.Components;

namespace Xabbo.Command.Modules;

[CommandModule(SupportedClients = ~ClientType.Shockwave)]
public sealed class ClickThroughCommand(ClickThroughComponent clickThrough) : CommandModule
{
    private readonly ClickThroughComponent _clickThrough = clickThrough;

    [Command("ct")]
    public Task OnToggleClickThrough(CommandArgs args)
    {
        _clickThrough.Enabled = !_clickThrough.Enabled;
        ShowMessage($"Click-through {(_clickThrough.Enabled ? "en" : "dis")}abled.");

        return Task.CompletedTask;
    }
}
