using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;

namespace Xabbo.Components;

[Intercept]
public partial class AntiTurnComponent(
    IExtension extension,
    IConfigProvider<AppConfig> config
    ) : Component(extension)
{
    private long _lastSelectedUser = -1;
    private int _lastLookAtX, _lastLookAtY;
    private DateTime _lastSelection = DateTime.MinValue;

    private readonly IConfigProvider<AppConfig> _config = config;
    private AppConfig Config => _config.Value;

    private bool Enabled => Config.Movement.NoTurn;
    private bool TurnOnReselect => Config.Movement.TurnOnReselectUser;
    private double ReselectThreshold => Config.Movement.ReselectThreshold;

    [Intercept]
    private void OnLookTo(Intercept e, LookToMsg look)
    {
        if (!Enabled) return;

        bool block = true;

        if (Client is ClientType.Shockwave)
        {
            if (Enabled && TurnOnReselect && (DateTime.Now - _lastSelection).TotalSeconds < ReselectThreshold)
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

        if (Enabled && TurnOnReselect && (DateTime.Now - _lastSelection).TotalSeconds < ReselectThreshold)
        {
            if (userId == _lastSelectedUser)
                Ext.Send(new LookToMsg(_lastLookAtX, _lastLookAtY));
        }

        _lastSelection = DateTime.Now;
        _lastSelectedUser = userId;
    }
}
