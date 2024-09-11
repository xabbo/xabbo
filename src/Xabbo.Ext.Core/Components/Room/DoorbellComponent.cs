using Microsoft.Extensions.Configuration;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Messages.Flash;
using Xabbo.Core.Game;

namespace Xabbo.Ext.Components;

[Intercept(~ClientType.Shockwave)]
public partial class DoorbellComponent : Component
{
    private readonly FriendManager _friendManager;

    private bool _acceptFriends;
    public bool AcceptFriends
    {
        get => _acceptFriends;
        set => Set(ref _acceptFriends, value);
    }

    public DoorbellComponent(
        IExtension extension,
        IConfiguration config,
        FriendManager friendManager)
        : base(extension)
    {
        _friendManager = friendManager;

        AcceptFriends = config.GetValue("Doorbell:AcceptFriends", false);
    }

    [InterceptIn(nameof(In.Doorbell))]
    protected void HandleDoorbellRinging(Intercept e)
    {
        string name = e.Packet.Read<string>();
        if (AcceptFriends && _friendManager.IsFriend(name))
        {
            e.Block();
            Ext.Send(Out.LetUserIn, name, true);
        }
    }
}
