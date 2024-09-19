using Xabbo.Extension;
using Xabbo.Messages.Flash;
using Xabbo.Core.Game;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;

namespace Xabbo.Components;

[Intercept(~ClientType.Shockwave)]
public partial class DoorbellComponent : Component
{
    private readonly IConfigProvider<AppConfig> _settings;
    private AppConfig Settings => _settings.Value;
    private readonly FriendManager _friendManager;

    public DoorbellComponent(
        IExtension extension,
        IConfigProvider<AppConfig> settings,
        FriendManager friendManager)
        : base(extension)
    {
        _settings = settings;
        _friendManager = friendManager;
    }

    [InterceptIn(nameof(In.Doorbell))]
    protected void HandleDoorbellRinging(Intercept e)
    {
        string name = e.Packet.Read<string>();
        if (Settings.Room.AcceptFriendsAtDoor &&
            _friendManager.IsFriend(name))
        {
            e.Block();
            Ext.Send(Out.LetUserIn, name, true);
        }
    }
}
