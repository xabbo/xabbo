using Microsoft.Extensions.Configuration;
using ReactiveUI;

using Xabbo.Messages;
using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Components;

[Intercept]
public partial class AntiIdleComponent : Component
{
    private readonly IConfiguration _config;

    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private int _pingCount = -1;

    private bool _isAntiIdleOutActive;
    public bool IsAntiIdleOutActive
    {
        get => _isAntiIdleOutActive;
        set => Set(ref _isAntiIdleOutActive, value);
    }

    public AntiIdleComponent(IExtension extension,
        IConfiguration config,
        ProfileManager profileManager,
        RoomManager roomManager)
        : base(extension)
    {
        _config = config;

        _profileManager = profileManager;
        _roomManager = roomManager;

        IsActive = config.GetValue("AntiIdle:Active", true);
        IsAntiIdleOutActive = config.GetValue("AntiIdle:AntiIdleOut", true);

        this.WhenAnyValue(x => x.IsActive, x => x.IsAntiIdleOutActive)
            .Subscribe(_ => OnActiveChanged());
    }

    protected override void OnConnected(GameConnectedArgs e)
    {
        base.OnConnected(e);
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();

        _pingCount = -1;
    }

    protected void OnActiveChanged() => SendAntiIdlePacket();

    protected override void OnInitialized(InitializedArgs e)
    {
        base.OnInitialized(e);

        IsActive = _config.GetValue("AntiIdle:Active", true);
        IsAntiIdleOutActive = _config.GetValue("AntiIdle:IdleOutActive", true);
    }

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
            if (IsActive)
                antiIdleMsg = new WalkMsg(0, 0);
        }
        else
        {
            if (_profileManager.UserData is not null &&
                _roomManager.Room is not null &&
                _roomManager.Room.TryGetUserById(_profileManager.UserData.Id, out IUser? self))
            {
                if (self.IsIdle && IsAntiIdleOutActive)
                    antiIdleMsg = new ActionMsg(Actions.Idle);
                else if (self.Dance != 0 && IsActive)
                    antiIdleMsg = new WalkMsg(0, 0);
                else if (IsActive)
                    antiIdleMsg = new ActionMsg(Actions.None);
            }
            else
            {
                antiIdleMsg = new ActionMsg(Actions.None);
            }
        }

        if (antiIdleMsg is not null)
            Ext.Send(antiIdleMsg);
    }
}
