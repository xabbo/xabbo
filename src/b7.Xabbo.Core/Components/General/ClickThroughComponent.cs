using System.ComponentModel;

using Microsoft.Extensions.Configuration;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Messages;

using b7.Xabbo.Commands;
using ReactiveUI;


namespace b7.Xabbo.Components;

public class ClickThroughComponent : Component
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

    protected override void OnInitialized(object? sender, ExtensionInitializedEventArgs e)
    {
        base.OnInitialized(sender, e);
    }

    private Task OnToggle(CommandArgs args)
    {
        IsActive = !IsActive;
        _commandManager.ShowMessage($"Click-through {(IsActive ? "enabled" : "disabled")}");

        return Task.CompletedTask;
    }

    [RequiredIn(nameof(Incoming.GameYouArePlayer))]
    protected void OnIsActiveChanged(bool isActive)
    {
        Extension.Send(In.GameYouArePlayer, isActive);
    }

    [InterceptIn(nameof(Incoming.RoomEntryInfo))]
    [RequiredIn(nameof(Incoming.GameYouArePlayer))]
    private void OnEnterRoom(InterceptArgs e)
    {
        if (IsActive)
        {
            Extension.Send(In.GameYouArePlayer, true);
        }
    }
}
