using Microsoft.Extensions.Configuration;

using Xabbo.Messages;
using Xabbo.Extension;

using Xabbo.Core.Game;

namespace b7.Xabbo.Components;

public class DoorbellComponent : Component
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

    [InterceptIn(nameof(Incoming.DoorbellRinging), RequiredClient = ClientType.Flash)]
    protected void HandleDoorbellRinging(InterceptArgs e)
    {
        if (Client != ClientType.Flash)
            return;

        string name = e.Packet.ReadString();
        if (AcceptFriends && _friendManager.IsFriend(name))
        {
            e.Block();
            Extension.Send(Out["LetUserIn"], name, true);
        }
    }
}
