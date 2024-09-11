using Microsoft.Extensions.Configuration;

using ReactiveUI;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Messages.Flash;

using Xabbo.Ext.Commands;

namespace Xabbo.Ext.Components;

[Intercept(~ClientType.Shockwave)]
public partial class ClickThroughComponent : Component
{
    protected readonly CommandManager _commandManager;

    public ClickThroughComponent(IExtension extension,
        IConfiguration config,
        CommandManager commandManager)
        : base(extension)
    {
        _commandManager = commandManager;
        _commandManager.Register(OnToggle, "ct", null);

        IsActive = config.GetValue("ClickThrough:Active", false);

        this.ObservableForProperty(x => x.IsActive)
            .Subscribe(x => OnIsActiveChanged(x.Value));
    }

    protected override void OnConnected(GameConnectedArgs e)
    {
        base.OnConnected(e);

        IsAvailable = Client is not ClientType.Shockwave;
    }

    private Task OnToggle(CommandArgs args)
    {
        IsActive = !IsActive;
        _commandManager.ShowMessage($"Click-through {(IsActive ? "enabled" : "disabled")}");

        return Task.CompletedTask;
    }

    // [RequiredIn(nameof(In.GameYouArePlayer))]
    protected void OnIsActiveChanged(bool isActive)
    {
        Ext.Send(In.YouArePlayingGame, isActive);
    }

    [InterceptIn(nameof(In.RoomEntryInfo))]
    // [RequiredIn(nameof(In.GameYouArePlayer))]
    private void OnEnterRoom(Intercept e)
    {
        if (IsActive)
            Ext.Send(In.YouArePlayingGame, true);
    }
}
