using ReactiveUI;

using Xabbo.Messages;
using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;

namespace Xabbo.Components;

[Intercept]
public partial class AntiIdleComponent : Component
{
    private readonly IConfigProvider<AppConfig> _settingsProvider;
    private AppConfig Settings => _settingsProvider.Value;

    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private int _pingCount = -1;

    public AntiIdleComponent(IExtension extension,
        IConfigProvider<AppConfig> settingsProvider,
        ProfileManager profileManager,
        RoomManager roomManager)
        : base(extension)
    {
        _profileManager = profileManager;
        _roomManager = roomManager;

        _settingsProvider = settingsProvider;
        _settingsProvider.WhenAnyValue(
            x => x.Value.General.AntiIdle,
            x => x.Value.General.AntiIdleOut)
            .Subscribe(_ => OnActiveChanged());
    }

    protected override void OnConnected(ConnectedEventArgs e)
    {
        base.OnConnected(e);
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();

        _pingCount = -1;
    }

    protected void OnActiveChanged() => SendAntiIdlePacket();

    [InterceptIn(nameof(In.Ping))]
    protected void HandlePing(Intercept e)
    {
        SendAntiIdlePacket();
        _pingCount++;
    }

    private void SendAntiIdlePacket()
    {
        if (_pingCount <= 0) return;

        IMessage? antiIdleMsg = null;

        if (Ext.Session.Client.Type is ClientType.Shockwave)
        {
            if (Settings.General.AntiIdle)
                antiIdleMsg = new WalkMsg(0, 0);
        }
        else
        {
            if (_profileManager.UserData is not null &&
                _roomManager.Room is not null &&
                _roomManager.Room.TryGetUserById(_profileManager.UserData.Id, out IUser? self))
            {
                if (self.IsIdle && Settings.General.AntiIdleOut)
                    antiIdleMsg = new ActionMsg(AvatarAction.Idle);
                else if (self.Dance != 0 && Settings.General.AntiIdle)
                    antiIdleMsg = new WalkMsg(0, 0);
                else if (Settings.General.AntiIdle)
                    antiIdleMsg = new ActionMsg(AvatarAction.None);
            }
            else
            {
                antiIdleMsg = new ActionMsg(AvatarAction.None);
            }
        }

        if (antiIdleMsg is not null)
            Ext.Send(antiIdleMsg);
    }
}
