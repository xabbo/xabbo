using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveUI;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;

using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Serialization;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;
using Xabbo.Models.Enums;
using Xabbo.ViewModels;
using System.Reactive.Linq;

namespace Xabbo.Components;

[Intercept]
public partial class RoomEntryComponent : Component
{
    private const int ERROR_INVALID_PW = -100002;

    private readonly ILogger _logger;
    private readonly IConfigProvider<AppConfig> _configProvider;
    private AppConfig Config => _configProvider.Value;
    private readonly IAppPathProvider _appPathProvider;

    private readonly Dictionary<long, string> _passwords = [];
    private Id _lastRequestedRoomId = -1;
    private DateTime _lastRequestedRoomTime = DateTime.Now;

    private bool _dontAskToRingDoorbell;
    public bool DontAskToRingDoorbell
    {
        get => _dontAskToRingDoorbell;
        set => Set(ref _dontAskToRingDoorbell, value);
    }

    public RoomEntryComponent(
        IExtension extension,
        ILoggerFactory loggerFactory,
        IConfigProvider<AppConfig> settings,
        IAppPathProvider appPathProvider,
        IDialogService dialogService)
        : base(extension)
    {
        _logger = loggerFactory.CreateLogger<RoomEntryComponent>();
        _configProvider = settings;
        _appPathProvider = appPathProvider;

        _configProvider.Loaded += OnSettingsLoaded;

        _configProvider
            .WhenAnyValue(x => x.Value.Room.RememberPasswords)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async rememberPasswords => {
                if (rememberPasswords && !Config.Room.RememberPasswordsConfirmed)
                {
                    string passwordsPath = appPathProvider.GetPath(AppPathKind.RoomPasswords);
                    await dialogService.ShowContentDialogAsync(
                        dialogService.CreateViewModel<MainViewModel>(), new ContentDialogSettings
                        {
                            Title = "Warning",
                            Content = $"Room passwords will be saved in plain-text in '{passwordsPath}'.",
                            PrimaryButtonText = "OK"
                        });
                    Config.Room.RememberPasswordsConfirmed = true;
                }
            });

        string passwordsFilePath = _appPathProvider.GetPath(AppPathKind.RoomPasswords);
        if (File.Exists(passwordsFilePath))
        {
            try
            {
                _passwords = JsonSerializer.Deserialize(
                    File.ReadAllText(passwordsFilePath),
                    JsonSourceGenerationContext.Default.DictionaryInt64String
                ) ?? [];
            }
            catch { }
        }
    }

    private void OnSettingsLoaded()
    {
        if (Config.Room.RememberPasswords && !Config.Room.RememberPasswordsConfirmed)
            Config.Room.RememberPasswords = false;
    }

    private void Save()
    {
        try
        {
            File.WriteAllText(
                _appPathProvider.GetPath(AppPathKind.RoomPasswords),
                JsonSerializer.Serialize(_passwords, JsonSourceGenerationContext.Default.DictionaryInt64String)
            );
        }
        catch { }
    }

    [InterceptIn(nameof(In.GetGuestRoomResult))]
    private void HandleGetGuestRoomResult(Intercept e)
    {
        var roomData = e.Packet.Read<RoomData>();

        if ((Config.Room.RememberPasswords && roomData.Access == RoomAccess.Password && _passwords.ContainsKey(roomData.Id)) ||
            (DontAskToRingDoorbell && roomData.Access == RoomAccess.Doorbell))
        {
            _logger.LogDebug("Rewriting room data for room #{RoomId}.", roomData.Id);

            if (Ext.Session.Is(ClientType.Shockwave))
            {
                roomData.Access = RoomAccess.Open;
            }
            else
            {
                roomData.IsGroupMember = true;
            }

            e.Packet.Clear();
            e.Packet.Write(roomData);
        }
    }

    [Intercept(ClientType.Shockwave)]
    [InterceptOut("TRYFLAT")]
    private void HandleTryFlat(Intercept e)
    {
        string[] split = e.Packet.ReadContent().Split('/');

        if (Config.Room.RememberPasswords &&
            split.Length > 0 &&
            Id.TryParse(split[0], out Id roomId))
        {
            _lastRequestedRoomId = roomId;
            _lastRequestedRoomTime = DateTime.Now;

            string? actualPassword = null;
            if (split.Length > 1)
                actualPassword = split[1];

            if (string.IsNullOrWhiteSpace(actualPassword))
            {
                if (_passwords.TryGetValue(roomId, out string? storedPassword))
                {
                    _logger.LogInformation("Rewriting password for room #{RoomId}.", roomId);
                    e.Packet.Position = 0;
                    e.Packet.WriteContent($"{roomId}/{storedPassword}");
                }
            }
            else
            {
                _logger.LogInformation("Storing password for room #{RoomId}.", roomId);
                _passwords.Add(roomId, split[1]);
                Save();
            }
        }
    }

    [Intercept(ClientType.Modern)]
    [InterceptOut(nameof(Out.OpenFlatConnection))]
    private void HandleFlatOpc(Intercept e)
    {
        _lastRequestedRoomId = e.Packet.Read<int>();
        string password = e.Packet.Read<string>();

        if (!Config.Room.RememberPasswords) return;

        if (!string.IsNullOrEmpty(password))
        {
            _passwords[_lastRequestedRoomId] = password;
            Save();
        }
        else
        {
            if (_passwords.ContainsKey(_lastRequestedRoomId))
            {
                password = _passwords[_lastRequestedRoomId];
                e.Packet.ReplaceAt<string>(4, password);
            }
        }
    }

    [Intercept(ClientType.Modern)]
    [InterceptIn(nameof(In.ErrorReport))]
    private void HandleError(Intercept e)
    {
        if (e.Packet.Read<int>() == ERROR_INVALID_PW)
            ResetInvalidPassword();
    }

    [Intercept(ClientType.Shockwave)]
    [InterceptIn("ERROR")]
    private void HandleErrorOrigins(Intercept e)
    {
        if (e.Packet.ReadContent() == "Incorrect flat password")
            ResetInvalidPassword();
    }

    private void ResetInvalidPassword()
    {
        if (!Config.Room.RememberPasswords) return;

        if (_lastRequestedRoomId > 0 && (DateTime.Now - _lastRequestedRoomTime).TotalSeconds < 5)
        {
            _logger.LogDebug("Resetting incorrect password for room #{RoomId}.", _lastRequestedRoomId);
            if (_passwords.Remove(_lastRequestedRoomId))
                Save();
        }
    }
}
