using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Interceptor;

namespace b7.Xabbo.ViewModel
{
    public class ChatLogViewManager : ComponentViewModel
    {
#if PRO
        const string LOG_DIRECTORY = @"b7\xabbo\logs\chat";
        private string currentFilePath;
#endif

        private readonly RoomManager _roomManager;

        private readonly StringBuilder _stringBuffer;

        private DateTime _lastDate = DateTime.MinValue;
        private long _lastRoom = -1;

        private StringBuilder _logText = new StringBuilder();
        public string LogText
        {
            get => _logText.ToString();
            set
            {
                _logText = new StringBuilder(value);
                RaisePropertyChanged(nameof(LogText));
            }
        }

        private bool _logToFile = true;
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
            _roomManager = roomManager;
            _roomManager.EntityChat += RoomManager_EntityChat;

            _stringBuffer = new StringBuilder();

            _antiBobba = config.GetValue<string>("AntiBobba:Inject");
        }

        private void OnInitialize()
        {
#if PRO
            Directory.CreateDirectory(LOG_DIRECTORY);
#endif
        }

        private void Log(string text)
        {
            _logText.Append(text);
            RaisePropertyChanged(nameof(LogText));
        }

        private void RoomManager_EntityChat(object? sender, EntityChatEventArgs e)
        {
            if (e.Entity.Type != EntityType.User) return;
            if (e.ChatType == ChatType.Whisper && e.BubbleStyle == 34) return;

            var today = DateTime.Today;
            if (today != _lastDate)
            {
                _lastDate = today;
#if PRO
                currentFilePath = Path.Combine(LOG_DIRECTORY, $"{today:yyyy-MM-dd}.txt");
#endif

                Log($"---------- {today:D} ----------\r\n");
            }

            IRoom? room = _roomManager.Room;
            if (room is null) return;

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

#if PRO
            if (LogToFile)
            {
                try { File.AppendAllText(currentFilePath, text); }
                catch (Exception ex)
                {
                    LogToFile = false;
                    Log($"[ERROR] Failed to log to file! {ex}");
                }
            }
#endif

            _stringBuffer.Clear();
        }
    }
}
