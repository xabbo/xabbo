using System.Text.Json;

using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Serialization;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;
using Xabbo.Models.Enums;

namespace Xabbo.Components;

[Intercept]
public partial class RoomEntryComponent : Component
{
    private const int ERROR_INVALID_PW = -100002;

    private readonly IConfigProvider<AppConfig> _settings;
    private AppConfig Settings => _settings.Value;
    private readonly IAppPathProvider _appPathProvider;

    private readonly Dictionary<long, string> _passwords = [];
    private int _lastRequestedRoom = -1;

    private bool _dontAskToRingDoorbell;
    public bool DontAskToRingDoorbell
    {
        get => _dontAskToRingDoorbell;
        set => Set(ref _dontAskToRingDoorbell, value);
    }

    public RoomEntryComponent(
        IExtension extension,
        IConfigProvider<AppConfig> settings,
        IAppPathProvider appPathProvider)
        : base(extension)
    {
        _settings = settings;
        _appPathProvider = appPathProvider;

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

        if ((Settings.Room.RememberPasswords && roomData.Access == RoomAccess.Password && _passwords.ContainsKey(roomData.Id)) ||
            (DontAskToRingDoorbell && roomData.Access == RoomAccess.Doorbell))
        {
            roomData.IsGroupMember = true;
            e.Packet.Clear();
            e.Packet.Write(roomData);
        }
    }

    [InterceptOut(nameof(Out.OpenFlatConnection))]
    private void HandleFlatOpc(Intercept e)
    {
        _lastRequestedRoom = e.Packet.Read<int>();
        string password = e.Packet.Read<string>();

        if (!Settings.Room.RememberPasswords) return;

        if (!string.IsNullOrEmpty(password))
        {
            _passwords[_lastRequestedRoom] = password;
            Save();
        }
        else
        {
            if (_passwords.ContainsKey(_lastRequestedRoom))
            {
                password = _passwords[_lastRequestedRoom];
                e.Packet.ReplaceAt<string>(4, password);
            }
        }
    }

    [InterceptIn(nameof(In.ErrorReport))]
    private void HandleError(Intercept e)
    {
        if (!Settings.Room.RememberPasswords) return;

        if (e.Packet.Read<int>() == ERROR_INVALID_PW &&
            _lastRequestedRoom != -1 &&
            _passwords.Remove(_lastRequestedRoom))
        {
            Save();
        }
    }
}
