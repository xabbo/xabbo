﻿using Microsoft.Extensions.Configuration;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Messages.Flash;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Incoming;

namespace b7.Xabbo.Components;

[Intercept(~ClientType.Shockwave)]
public partial class RespectedComponent : Component
{
    private readonly RoomManager _roomManager;

    private DateTime _lastRespect = DateTime.MinValue;
    private int _lastRespecterIndex = -1;

    private bool _showWhoRespected = true;
    public bool ShowWhoRespected
    {
        get => _showWhoRespected;
        set => Set(ref _showWhoRespected, value);
    }

    private bool _showTotalRespects = true;
    public bool ShowTotalRespects
    {
        get => _showTotalRespects;
        set => Set(ref _showTotalRespects, value);
    }

    public RespectedComponent(IExtension extension,
        IConfiguration config,
        RoomManager roomManager)
        : base(extension)
    {
        ShowWhoRespected = config.GetValue("Respect:ShowWhoRespected", true);
        ShowTotalRespects = config.GetValue("Respect:ShowTotalRespects", true);

        _roomManager = roomManager;
        roomManager.Left += Room_Left;
    }

    private void Room_Left()
    {
        _lastRespect = DateTime.MinValue;
        _lastRespecterIndex = -1;
    }

    [InterceptIn(nameof(In.Expression))]
    private void InUserAction(Intercept e)
    {
        if (!_roomManager.IsInRoom)
            return;

        int index = e.Packet.Read<int>();
        Actions action = (Actions)e.Packet.Read<int>();

        if (action == Actions.ThumbsUp)
        {
            _lastRespect = DateTime.Now;
            _lastRespecterIndex = index;
        }
    }

    [InterceptIn(nameof(In.RespectNotification))]
    private void HandleRespectNotification(Intercept e)
    {
        IRoom? room = _roomManager.Room;
        if (room is null || (!ShowWhoRespected && !ShowTotalRespects))
            return;

        int id = e.Packet.Read<int>();
        int totalRespects = e.Packet.Read<int>();

        IUser? respectee = room.GetAvatarById<IUser>(id);
        if (respectee == null)
            return;

        e.Block();

        string message = $"{respectee.Name} was respected";

        if (_lastRespecterIndex > -1 && (DateTime.Now - _lastRespect).TotalMilliseconds < 500)
        {
            if (ShowWhoRespected)
            {
                IUser? respecter = room.GetAvatar<IUser>(_lastRespecterIndex);
                if (respecter is not null)
                    message += $" by {respecter.Name}";
            }

            _lastRespecterIndex = -1;
        }

        message += "!";
        if (ShowTotalRespects)
            message += $" ({totalRespects})";

        Ext.Send(new AvatarWhisperMsg(respectee.Index, message, 1));
    }
}
