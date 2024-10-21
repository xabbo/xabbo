using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Splat;
using Avalonia.Controls.Selection;
using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Configuration;
using Xabbo.Services.Abstractions;

using IconSource = FluentAvalonia.UI.Controls.IconSource;

namespace Xabbo.ViewModels;

public class ChatPageViewModel : PageViewModel
{
    public override string Header => "Chat";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Chat };

    private readonly IConfigProvider<AppConfig> _settingsProvider;
    private AppConfig Settings => _settingsProvider.Value;

    private readonly IGameStateService _gameState;
    private readonly IFigureConverterService _figureConverter;

    private readonly RoomManager _roomManager;
    private readonly ProfileManager _profileManager;

    public ChatLogConfig Config => Settings.Chat.Log;

    private long _currentMessageId;
    private readonly SourceCache<ChatLogEntryViewModel, long> _cache = new(x => x.EntryId);

    private readonly ReadOnlyObservableCollection<ChatLogEntryViewModel> _messages;
    public ReadOnlyObservableCollection<ChatLogEntryViewModel> Messages => _messages;

    public SelectionModel<ChatLogEntryViewModel> Selection { get; } = new SelectionModel<ChatLogEntryViewModel>()
    {
        SingleSelect = false
    };

    [Reactive] public string? FilterText { get; set; }

    [DependencyInjectionConstructor]
    public ChatPageViewModel(
        IConfigProvider<AppConfig> settingsProvider,
        IGameStateService gameState,
        IFigureConverterService figureConverter,
        RoomManager roomManager,
        ProfileManager profileManager)
    {
        _settingsProvider = settingsProvider;
        _gameState = gameState;
        _figureConverter = figureConverter;
        _roomManager = roomManager;
        _profileManager = profileManager;

        _roomManager.Entered += OnEnteredRoom;
        _roomManager.AvatarAdded += OnAvatarAdded;
        _roomManager.AvatarRemoved += OnAvatarRemoved;
        _roomManager.AvatarChat += RoomManager_AvatarChat;
        _roomManager.AvatarUpdated += RoomManager_AvatarUpdated;

        _cache
            .Connect()
            .Filter(this
                .WhenAnyValue(x => x.FilterText)
                .Select(CreateFilter)
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .SortAndBind(out _messages, SortExpressionComparer<ChatLogEntryViewModel>.Ascending(x => x.EntryId))
            .Subscribe();
    }

    private long NextEntryId() => Interlocked.Increment(ref _currentMessageId);

    private static Func<ChatLogEntryViewModel, bool> CreateFilter(string? filterText)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return _ => true;
        }
        else
        {
            return (vm) => vm switch
            {
                ChatMessageViewModel chat =>
                    chat.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
                    chat.Message.Contains(filterText, StringComparison.OrdinalIgnoreCase),
                ChatLogAvatarActionViewModel action =>
                    action.UserName.Contains(filterText, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }
    }

    private void AppendLog(ChatLogEntryViewModel vm)
    {
        vm.EntryId = NextEntryId();
        _cache.AddOrUpdate(vm);
    }

    private void OnAvatarAdded(AvatarEventArgs e)
    {
        if (_roomManager.IsLoadingRoom || !Config.UserEntry || e.Avatar.Type is not AvatarType.User)
            return;

        AppendLog(new ChatLogAvatarActionViewModel
        {
            UserName = e.Avatar.Name,
            Action = "entered the room"
        });
    }

    private void OnAvatarRemoved(AvatarEventArgs e)
    {
        if (_roomManager.IsLoadingRoom || !Config.UserEntry || e.Avatar.Type is not AvatarType.User)
            return;

        AppendLog(new ChatLogAvatarActionViewModel
        {
            UserName = e.Avatar.Name.Equals(_profileManager.UserData?.Name) ? "You" : e.Avatar.Name,
            Action = "left the room"
        });
    }

    private void RoomManager_AvatarUpdated(AvatarEventArgs e)
    {
        if (Config.Trades &&
            e.Avatar.PreviousUpdate is { IsTrading: bool wasTrading } &&
            e.Avatar.CurrentUpdate is { IsTrading: bool isTrading } &&
            isTrading != wasTrading)
        {
            if (isTrading)
            {
                AppendLog(new ChatLogAvatarActionViewModel
                {
                    UserName = e.Avatar.Name,
                    Action = "started trading"
                });
            }
            else
            {
                AppendLog(new ChatLogAvatarActionViewModel
                {
                    UserName = e.Avatar.Name,
                    Action = "stopped trading"
                });
            }
        }
    }

    private void OnEnteredRoom(RoomEventArgs e) => AppendLog(new ChatLogRoomEntryViewModel
    {
        RoomName = e.Room.Data?.Name ?? "?",
        RoomOwner = e.Room.Data?.OwnerName ?? "?"
    });

    private void RoomManager_AvatarChat(AvatarChatEventArgs e)
    {
        if (!Settings.Chat.Log.Normal && e is { Avatar.Type: AvatarType.User, ChatType: not ChatType.Whisper }) return;
        if (!Settings.Chat.Log.Whispers && e is { ChatType: ChatType.Whisper, BubbleStyle: not 34 }) return;
        if (!Settings.Chat.Log.Bots && e.Avatar.Type is AvatarType.PublicBot or AvatarType.PrivateBot) return;
        if (!Settings.Chat.Log.Pets && e.Avatar.Type is AvatarType.Pet) return;
        if (!Settings.Chat.Log.Wired && e is { ChatType: ChatType.Whisper, BubbleStyle: 34 }) return;

        IRoom? room = _roomManager.Room;
        if (room is null) return;

        string? figureString = null;
        if (e.Avatar.Type is not AvatarType.Pet)
        {
            if (_gameState.Session.Is(ClientType.Origins))
            {
                if (_figureConverter.TryConvertToModern(e.Avatar.Figure, out Figure? figure))
                    figureString = figure.ToString();
            }
            else
            {
                figureString = e.Avatar.Figure;
            }
        }

        AppendLog(new ChatMessageViewModel
        {
            Type = e.ChatType,
            BubbleStyle = e.BubbleStyle,
            Name = e.Avatar.Name,
            Message = H.RenderText(e.Message),
            FigureString = figureString,
        });
    }
}
