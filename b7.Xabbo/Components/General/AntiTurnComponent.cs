using System;

using Microsoft.Extensions.Configuration;

using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components;

public class AntiTurnComponent : Component
{
    private readonly double _reselectThreshold;

    private long _lastSelectedUser = -1;
    private int _lastLookAtX, _lastLookAtY;
    private DateTime _lastSelection = DateTime.MinValue;

    private bool _turnOnReselect = true;
    public bool TurnOnReselect
    {
        get => _turnOnReselect;
        set => Set(ref _turnOnReselect, value);
    }

    public AntiTurnComponent(IInterceptor interceptor,
        IConfiguration config)
        : base(interceptor)
    {
        _reselectThreshold = config.GetValue("AntiTurn:ReselectThreshold", 1.0);

        IsActive = config.GetValue("AntiTurn:Active", true);
        TurnOnReselect = config.GetValue("AntiTurn:TurnOnReselect", true);
    }

    [InterceptOut(nameof(Outgoing.LookTo))]
    private void OnLookTo(InterceptArgs e)
    {
        if (IsActive) e.Block();
        _lastLookAtX = e.Packet.ReadInt();
        _lastLookAtY = e.Packet.ReadInt();
    }

    [InterceptOut(nameof(Outgoing.GetSelectedBadges))]
    private void OnRequestWearingBadges(InterceptArgs e)
    {
        long userId = e.Packet.ReadLegacyLong();

        if (IsActive && TurnOnReselect && (DateTime.Now - _lastSelection).TotalSeconds < _reselectThreshold)
        {
            if (userId == _lastSelectedUser)
                Interceptor.Send(Out.LookTo, _lastLookAtX, _lastLookAtY);
        }

        _lastSelection = DateTime.Now;
        _lastSelectedUser = userId;
    }
}
