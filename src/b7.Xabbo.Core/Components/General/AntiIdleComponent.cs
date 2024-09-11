using Microsoft.Extensions.Configuration;
using ReactiveUI;

using Xabbo;
using Xabbo.Extension;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Messages.Flash;

namespace b7.Xabbo.Components;

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
        _pingCount = e.Packet.Read<int>();

        if (_pingCount > 0 && _pingCount % 6 == 0)
        {
            SendAntiIdlePacket();
        }
    }

    private void SendAntiIdlePacket()
    {
        if (_pingCount <= 0) return;

        if (_profileManager.UserData is not null &&
            _roomManager.Room is not null &&
            _roomManager.Room.TryGetUserById(_profileManager.UserData.Id, out IUser? self))
        {
            if (self.IsIdle && IsAntiIdleOutActive)
            {
                Ext.Send(Out.AvatarExpression, 5);
            }
            else if (self.Dance != 0 && IsActive)
            {
                Ext.Send(Out.MoveAvatar, 0, 0);
            }
            else if (IsActive)
            {
                Ext.Send(Out.AvatarExpression, 0);
            }
        }
        else
        {
            if (IsActive)
            {
                Ext.Send(Out.AvatarExpression, 0);
            }
        }
    }
}
