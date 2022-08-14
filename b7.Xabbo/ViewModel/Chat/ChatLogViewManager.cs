using System;
using System.IO;
using System.Text;

using Microsoft.Extensions.Configuration;

using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Interceptor;

namespace b7.Xabbo.ViewModel;

public class ChatLogViewManager : ComponentViewModel
{
    const string LogDirectory = @"logs\chat";
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
            RaisePropertyChanged(nameof(LogText));
        }
    }

    private bool _includeNormalChat;
    public bool IncludeNormalChat
    {
        get => _includeNormalChat;
        set => Set(ref _includeNormalChat, value);
    }

    private bool _includeWhispers;
    public bool IncludeWhispers
    {
        get => _includeWhispers;
        set => Set(ref _includeWhispers, value);
    }

    private bool _includeWiredMessages;
    public bool IncludeWiredMessages
    {
        get => _includeWiredMessages;
        set => Set(ref _includeWiredMessages, value);
    }

    private bool _includeBotMessages;
    public bool IncludeBotMessages
    {
        get => _includeBotMessages;
        set => Set(ref _includeBotMessages, value);
    }

    private bool _logToFile;
    public bool LogToFile
    {
        get => _logToFile;
        set => Set(ref _logToFile, value);
    }

    private readonly string? _antiBobba;

    public ChatLogViewManager(IInterceptor interceptor,
        IConfiguration config,
        RoomManager roomManager)
        : base(interceptor)
    {
        Directory.CreateDirectory(LogDirectory);

        _roomManager = roomManager;
        _roomManager.EntityChat += RoomManager_EntityChat;

        _stringBuffer = new StringBuilder();

        _antiBobba = config.GetValue<string>("AntiBobba:Inject");

        _includeNormalChat = config.GetValue("ChatLog:Normal", true);
        _includeWhispers = config.GetValue("ChatLog:Whispers", true);
        _includeWiredMessages = config.GetValue("ChatLog:Wired", false);
        _includeBotMessages = config.GetValue("ChatLog:Bots", false);
        _logToFile = config.GetValue("ChatLog:LogToFile", false);
    }

    private void Log(string text)
    {
        _logText.Append(text);
        RaisePropertyChanged(nameof(LogText));
    }

    private void RoomManager_EntityChat(object? sender, EntityChatEventArgs e)
    {
        if (!IncludeNormalChat && e.Entity.Type == EntityType.User && e.ChatType != ChatType.Whisper) return;
        if (!IncludeWhispers && e.ChatType == ChatType.Whisper && e.BubbleStyle != 34) return;
        if (!IncludeBotMessages && (e.Entity.Type == EntityType.PublicBot || e.Entity.Type == EntityType.PrivateBot)) return;
        if (!IncludeWiredMessages && e.ChatType == ChatType.Whisper && e.BubbleStyle == 34) return;
        if (e.Entity.Type == EntityType.Pet) return;

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
        if (!string.IsNullOrWhiteSpace(_antiBobba))
            message = message.Replace(_antiBobba, "");

        _stringBuffer.AppendFormat(
            "[{0:HH:mm:ss}] {1}{2}: {3}",
            DateTime.Now,
            e.ChatType == ChatType.Whisper ? "* " : "",
            e.Entity.Name,
            message
        );
        _stringBuffer.AppendLine();

        string text = _stringBuffer.ToString();

        Log(text);

        if (LogToFile && !string.IsNullOrWhiteSpace(_currentFilePath))
        {
            try { File.AppendAllText(_currentFilePath, text); }
            catch (Exception ex)
            {
                LogToFile = false;
                Log($"[ERROR] Failed to log to file! {ex}");
            }
        }

        _stringBuffer.Clear();
    }
}
