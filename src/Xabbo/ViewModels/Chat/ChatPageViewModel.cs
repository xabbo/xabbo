using System.Text;
using ReactiveUI;
using Splat;
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

    const string LogDirectory = "logs/chat";
    private string? _currentFilePath;

    private readonly RoomManager _roomManager;

    private readonly StringBuilder _stringBuffer;

    private DateTime _lastDate = DateTime.MinValue;
    private long _lastRoom = -1;

    private StringBuilder _logText = new();
    public string LogText
    {
        get => _logText.ToString();
        set
        {
            _logText = new(value);
            this.RaisePropertyChanged(nameof(LogText));
        }
    }

    public Configuration.ChatLogSettings Config => Settings.Chat.Log;

    [DependencyInjectionConstructor]
    public ChatPageViewModel(
        IConfigProvider<AppConfig> settingsProvider,
        RoomManager roomManager)
    {
        _settingsProvider = settingsProvider;
        _roomManager = roomManager;
        _roomManager.AvatarChat += RoomManager_AvatarChat;

        _stringBuffer = new StringBuilder();

        Directory.CreateDirectory(LogDirectory);
    }

    private void Log(string text)
    {
        _logText.Append(text);
        this.RaisePropertyChanged(nameof(LogText));
    }

    private void RoomManager_AvatarChat(AvatarChatEventArgs e)
    {
        if (!Settings.Chat.Log.Normal && e.Avatar.Type == AvatarType.User && e.ChatType != ChatType.Whisper) return;
        if (!Settings.Chat.Log.Whispers && e.ChatType == ChatType.Whisper && e.BubbleStyle != 34) return;
        if (!Settings.Chat.Log.Bots && (e.Avatar.Type == AvatarType.PublicBot || e.Avatar.Type == AvatarType.PrivateBot)) return;
        if (!Settings.Chat.Log.Pets && (e.Avatar.Type == AvatarType.Pet)) return;
        if (!Settings.Chat.Log.Wired && e.ChatType == ChatType.Whisper && e.BubbleStyle == 34) return;

        IRoom? room = _roomManager.Room;
        if (room is null) return;

        DateTime today = DateTime.Today;
        if (today != _lastDate)
        {
            _lastDate = today;
            _currentFilePath = Path.Combine(LogDirectory, $"{today:yyyy-MM-dd}.txt");

            Log($"---------- {today:D} ----------\r\n");
        }

        if (_lastRoom != room.Id)
        {
            _lastRoom = room.Id;

            _stringBuffer.AppendLine();
            _stringBuffer.AppendFormat(
                "---------- {0} (id:{1}) ----------",
                H.RenderText(room.Data?.Name ?? "?"),
                room.Id
            );
            _stringBuffer.AppendLine();
        }

        string message = H.RenderText(e.Message);

        _stringBuffer.AppendFormat(
            "[{0:HH:mm:ss}] {1}{2}: {3}",
            DateTime.Now,
            e.ChatType == ChatType.Whisper ? "* " : "",
            e.Avatar.Name,
            message
        );
        _stringBuffer.AppendLine();

        string text = _stringBuffer.ToString();

        Log(text);

        if (Settings.Chat.Log.LogToFile && !string.IsNullOrWhiteSpace(_currentFilePath))
        {
            try { File.AppendAllText(_currentFilePath, text); }
            catch (Exception ex)
            {
                Settings.Chat.Log.LogToFile = false;
                Log($"[ERROR] Failed to log to file! {ex}");
            }
        }

        _stringBuffer.Clear();
    }
}
