using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Messages.Flash;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;

using Xabbo.Ext.Configuration;

namespace Xabbo.Ext.Components;

[Intercept]
public partial class ChatComponent : Component
{
    private readonly RoomManager _roomManager;
    private readonly HashSet<string> _petCommands;

    private bool _mutePets;
    public bool MutePets
    {
        get => _mutePets;
        set => Set(ref _mutePets, value);
    }

    private bool _mutePetCommands;
    public bool MutePetCommands
    {
        get => _mutePetCommands;
        set => Set(ref _mutePetCommands, value);
    }

    private bool _muteBots;
    public bool MuteBots
    {
        get => _muteBots;
        set => Set(ref _muteBots, value);
    }

    private bool _muteWired;
    public bool MuteWired
    {
        get => _muteWired;
        set => Set(ref _muteWired, value);
    }

    private bool _muteRespects;
    public bool MuteRespects
    {
        get => _muteRespects;
        set => Set(ref _muteRespects, value);
    }

    private bool _muteScratches;
    public bool MuteScratches
    {
        get => _muteScratches;
        set => Set(ref _muteScratches, value);
    }

    public ChatComponent(IExtension extension,
        IConfiguration config,
        IOptions<GameOptions> gameOptions,
        RoomManager roomManager)
        : base(extension)
    {
        _roomManager = roomManager;
        _roomManager.AvatarChat += OnAvatarChat;

        _petCommands = gameOptions.Value.PetCommands;

        MutePets = config.GetValue("Chat:Mute:Pets", false);
        MutePetCommands = config.GetValue("Chat:Mute:PetCommands", false);
        MuteBots = config.GetValue("Chat:Mute:Bots", false);
        MuteWired = config.GetValue("Chat:Mute:Wired", false);
        MuteRespects = config.GetValue("Chat:Mute:Respects", false);
        MuteScratches = config.GetValue("Chat:Mute:Scratches", false);
    }

    private void OnAvatarChat(AvatarChatEventArgs e)
    {
        if (MutePets && e.Avatar.Type == AvatarType.Pet)
        {
            e.Block();
            return;
        }

        if (MuteBots)
        {
            if (e.Avatar.Type == AvatarType.PublicBot ||
                e.Avatar.Type == AvatarType.PrivateBot)
            {
                e.Block();
                return;
            }
        }

        if (MutePetCommands)
        {
            int index = e.Message.IndexOf(' ');
            if (index > 0)
            {
                string name = e.Message[..index];
                if (name == "bobba" || _roomManager.Room?.GetAvatar<IPet>(name) is not null)
                {
                    string command = e.Message[(index + 1)..].ToLower();
                    if (_petCommands.Contains(command))
                    {
                        e.Block();
                        return;
                    }
                }
            }
        }

        if (MuteWired && e.BubbleStyle == 34) e.Block();
    }

    [Intercept(~ClientType.Shockwave)]
    [InterceptIn(nameof(In.RespectNotification))]
    private void OnUserRespect(Intercept e)
    {
        if (MuteRespects) e.Block();
    }

    [Intercept(~ClientType.Shockwave)]
    [InterceptIn(nameof(In.PetRespectNotification))]
    private void OnRoomPetRespect(Intercept e)
    {
        if (MuteScratches) e.Block();
    }
}
