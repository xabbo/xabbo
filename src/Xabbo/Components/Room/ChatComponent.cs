using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;

namespace Xabbo.Components;

[Intercept]
public partial class ChatComponent : Component
{
    private readonly RoomManager _roomManager;
    private readonly IConfigProvider<AppConfig> _settingsProvider;

    private AppConfig Settings => _settingsProvider.Value;

    public ChatComponent(
        IConfigProvider<AppConfig> settingsProvider,
        IExtension extension,
        RoomManager roomManager)
        : base(extension)
    {
        _settingsProvider = settingsProvider;

        _roomManager = roomManager;
        _roomManager.AvatarChat += OnAvatarChat;

        _settingsProvider = settingsProvider;
    }

    private void OnAvatarChat(AvatarChatEventArgs e)
    {
        if (Settings.Chat.MutePets && e.Avatar.Type == AvatarType.Pet)
        {
            e.Block();
            return;
        }

        if (Settings.Chat.MuteBots)
        {
            if (e.Avatar.Type == AvatarType.PublicBot ||
                e.Avatar.Type == AvatarType.PrivateBot)
            {
                e.Block();
                return;
            }
        }

        if (Settings.Chat.MutePetCommands)
        {
            int index = e.Message.IndexOf(' ');
            if (index > 0)
            {
                string name = e.Message[..index];
                if (name == "bobba" || _roomManager.Room?.GetAvatar<IPet>(name) is not null)
                {
                    string command = e.Message[(index + 1)..].ToLower();
                    if (Settings.Chat.PetCommands.Contains(command))
                    {
                        e.Block();
                        return;
                    }
                }
            }
        }

        if (Settings.Chat.MuteWired && e.BubbleStyle == 34) e.Block();
    }

    [Intercept(~ClientType.Shockwave)]
    [InterceptIn(nameof(In.RespectNotification))]
    private void OnUserRespect(Intercept e)
    {
        if (Settings.Chat.MuteRespects) e.Block();
    }

    [Intercept(~ClientType.Shockwave)]
    [InterceptIn(nameof(In.PetRespectNotification))]
    private void OnRoomPetRespect(Intercept e)
    {
        if (Settings.Chat.MuteScratches) e.Block();
    }
}
