using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;
using Xabbo.Core.GameData;

namespace Xabbo.Components;

[Intercept]
public partial class ChatComponent : Component
{
    private readonly RoomManager _roomManager;
    private readonly IGameDataManager _gameData;
    private readonly IConfigProvider<AppConfig> _settingsProvider;

    private AppConfig Settings => _settingsProvider.Value;

    private HashSet<string> _petCommands = new(StringComparer.OrdinalIgnoreCase);

    public ChatComponent(
        IExtension extension,
        IConfigProvider<AppConfig> settingsProvider,
        IGameDataManager gameData,
        RoomManager roomManager)
        : base(extension)
    {
        _settingsProvider = settingsProvider;
        _gameData = gameData;

        _roomManager = roomManager;
        _roomManager.AvatarChat += OnAvatarChat;

        _settingsProvider = settingsProvider;

        _gameData.Loaded += OnGameDataLoaded;
    }

    private void OnGameDataLoaded()
    {
        HashSet<string> petCommands = new(StringComparer.OrdinalIgnoreCase);
        if (_gameData.Texts is { } texts)
        {
            foreach (var (key, value) in texts)
            {
                if (key.StartsWith("pet.command."))
                {
                    petCommands.Add(value);
                }
            }
        }

        _petCommands = petCommands;
    }

    private void OnAvatarChat(AvatarChatEventArgs e)
    {
        if (Settings.Chat.MuteAll)
        {
            e.Block();
            return;
        }

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
                    string command = e.Message[(index + 1)..];
                    if (_petCommands.Contains(command))
                    {
                        e.Block();
                        return;
                    }
                }
            }
        }

        if (Settings.Chat.MuteWired && e.BubbleStyle == 34) e.Block();
    }

    [InterceptOut(nameof(Out.Chat))]
    private void OnChat(Intercept e)
    {
        if (Settings.Chat.AlwaysShout)
        {
            e.Packet.Header = Ext.Messages.Resolve(Out.Shout);
        }
    }

    [Intercept(ClientType.Modern)]
    [InterceptIn(nameof(In.RespectNotification))]
    private void OnUserRespect(Intercept e)
    {
        if (Settings.Chat.MuteRespects) e.Block();
    }

    [Intercept(ClientType.Modern)]
    [InterceptIn(nameof(In.PetRespectNotification))]
    private void OnRoomPetRespect(Intercept e)
    {
        if (Settings.Chat.MuteScratches) e.Block();
    }
}
