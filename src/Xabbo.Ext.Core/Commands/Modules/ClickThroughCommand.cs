using Xabbo.Ext.Components;

namespace Xabbo.Ext.Commands;

[CommandModule(SupportedClients = ~ClientType.Shockwave)]
public sealed class ClickThroughCommand(ClickThroughComponent clickThrough) : CommandModule
{
    private readonly ClickThroughComponent _clickThrough = clickThrough;

    [Command("ct")]
    public Task OnToggleClickThrough(CommandArgs args)
    {
        _clickThrough.IsActive = !_clickThrough.IsActive;
        ShowMessage($"Click-through {(_clickThrough.IsActive ? "en" : "dis")}abled.");

        return Task.CompletedTask;
    }
}
