using Microsoft.Extensions.Configuration;

using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Components;

[Intercept]
public partial class AntiTurnComponent : Component
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

    public AntiTurnComponent(IExtension extension,
        IConfiguration config)
        : base(extension)
    {
        _reselectThreshold = config.GetValue("AntiTurn:ReselectThreshold", 1.0);

        IsActive = config.GetValue("AntiTurn:Active", true);
        TurnOnReselect = config.GetValue("AntiTurn:TurnOnReselect", true);
    }

    [Intercept]
    private void OnLookTo(Intercept e, LookToMsg look)
    {
        if (!IsActive) return;

        bool block = true;

        if (Client is ClientType.Shockwave)
        {
            if (IsActive && TurnOnReselect && (DateTime.Now - _lastSelection).TotalSeconds < _reselectThreshold)
            {
                if (_lastLookAtX == look.X && _lastLookAtY == look.Y)
                    block = false;
            }

            _lastSelection = DateTime.Now;
        }

        if (block) e.Block();

        _lastLookAtX = look.X;
        _lastLookAtY = look.Y;
    }

    [Intercept(~ClientType.Shockwave)]
    [InterceptOut(nameof(Out.GetSelectedBadges))]
    private void OnRequestWearingBadges(Intercept e)
    {
        Id userId = e.Packet.Read<Id>();

        if (IsActive && TurnOnReselect && (DateTime.Now - _lastSelection).TotalSeconds < _reselectThreshold)
        {
            if (userId == _lastSelectedUser)
                Ext.Send(new LookToMsg(_lastLookAtX, _lastLookAtY));
        }

        _lastSelection = DateTime.Now;
        _lastSelectedUser = userId;
    }
}
