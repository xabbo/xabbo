using System;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;

using Xabbo;
using Xabbo.Messages;
using Xabbo.Extension;

using Xabbo.Core;
using Xabbo.Core.Game;
using ReactiveUI;
using DynamicData.Binding;

namespace b7.Xabbo.Components;

public class AntiIdleComponent : Component
{
    private readonly IConfiguration _config;

    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private int _latencyCheckCount = -1;

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

    protected override void OnDisconnected(object? sender, EventArgs e)
    {
        base.OnDisconnected(sender, e);

        _latencyCheckCount = -1;
    }

    protected void OnActiveChanged() => SendAntiIdlePacket();

    protected override void OnInitialized(object? sender, ExtensionInitializedEventArgs e)
    {
        base.OnInitialized(sender, e);

        IsActive = _config.GetValue("AntiIdle:Active", true);
        IsAntiIdleOutActive = _config.GetValue("AntiIdle:IdleOutActive", true);
    }

    [InterceptIn(nameof(Incoming.ClientLatencyPingResponse))]
    protected void HandleClientLatencyPingResponse(InterceptArgs e)
    {
        _latencyCheckCount = e.Packet.ReadInt();

        if (_latencyCheckCount > 0 &&
            _latencyCheckCount % 12 == 0)
        {
            SendAntiIdlePacket();
        }
    }

    private void SendAntiIdlePacket()
    {
        if (_latencyCheckCount <= 0) return;

        if (_profileManager.UserData is not null &&
            _roomManager.Room is not null &&
            _roomManager.Room.TryGetUserById(_profileManager.UserData.Id, out IRoomUser? self))
        {
            if (self.IsIdle && IsAntiIdleOutActive)
            {
                Extension.Send(Out.Expression, 5);
            }
            else if (self.Dance != 0 && IsActive)
            {
                Extension.Send(Out.Move, 0, 0);
            }
            else if (IsActive)
            {
                Extension.Send(Out.Expression, 0);
            }
        }
        else
        {
            if (IsActive)
            {
                Extension.Send(Out.Expression, 0);
            }
        }
    }
}
