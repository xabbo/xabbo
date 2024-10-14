using Xabbo.Extension;
using Xabbo.Core.Game;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Components;

[Intercept]
public partial class DoorbellComponent(
    IExtension extension,
    IConfigProvider<AppConfig> config,
    XabbotComponent xabbot,
    FriendManager friendManager) : Component(extension)
{
    private readonly IConfigProvider<AppConfig> _config = config;
    private AppConfig Config => _config.Value;
    private readonly XabbotComponent _xabbot = xabbot;
    private readonly FriendManager _friendManager = friendManager;

    [Intercept]
    protected void HandleDoorbell(Intercept<DoorbellMsg> e)
    {
        if (Config.Room.AcceptFriendsAtDoor &&
            _friendManager.IsFriend(e.Msg.Name))
        {
            e.Block();
            _xabbot.ShowMessage($"Accepting {e.Msg.Name} at door.");
            Ext.Send(new AnswerDoorbellMsg(e.Msg.Name, true));
        }
    }
}
