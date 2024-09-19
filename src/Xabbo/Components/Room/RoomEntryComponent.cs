using System.Text.Json;
using Microsoft.Extensions.Configuration;

using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;

namespace Xabbo.Components;

[Intercept]
public partial class RoomEntryComponent : Component
{
    private Dictionary<long, string> _passwords = new();

    private const string FILE_PATH = @"passwords.json";
    private const int ERROR_INVALID_PW = -100002;

    private int _lastRequestedRoom = -1;

    private bool _rememberPasswords;
    public bool RememberPasswords
    {
        get => _rememberPasswords;
        set => Set(ref _rememberPasswords, value);
    }

    private bool _dontAskToRingDoorbell;
    public bool DontAskToRingDoorbell
    {
        get => _dontAskToRingDoorbell;
        set => Set(ref _dontAskToRingDoorbell, value);
    }

    public RoomEntryComponent(IExtension extension,
        IConfiguration config)
        : base(extension)
    {
        RememberPasswords = config.GetValue("RoomEntry:RememberPasswords", true);

        if (File.Exists(FILE_PATH))
        {
            try
            {
                _passwords = JsonSerializer.Deserialize<Dictionary<long, string>>(
                    File.ReadAllText(FILE_PATH)
                ) ?? new();
            }
            catch { }
        }
    }

    private void Save()
    {
        try
        {
            File.WriteAllText(FILE_PATH, JsonSerializer.Serialize(_passwords));
        }
        catch { }
    }

    [InterceptIn(nameof(In.GetGuestRoomResult))]
    private void HandleGetGuestRoomResult(Intercept e)
    {
        var roomData = e.Packet.Read<RoomData>();

        if ((RememberPasswords && roomData.Access == RoomAccess.Password && _passwords.ContainsKey(roomData.Id)) ||
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

        if (!RememberPasswords) return;

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
        if (!RememberPasswords) return;

        if (e.Packet.Read<int>() == ERROR_INVALID_PW &&
            _lastRequestedRoom != -1 &&
            _passwords.Remove(_lastRequestedRoom))
        {
            Save();
        }
    }
}
