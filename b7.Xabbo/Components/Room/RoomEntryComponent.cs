using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using Microsoft.Extensions.Configuration;

using Xabbo.Messages;
using Xabbo.Interceptor;
using Xabbo.Core;

namespace b7.Xabbo.Components;

public class RoomEntryComponent : Component
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

    public RoomEntryComponent(IInterceptor interceptor,
        IConfiguration config)
        : base(interceptor)
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

    [InterceptIn(nameof(Incoming.GetGuestRoomResult))]
    private void HandleGetGuestRoomResult(InterceptArgs e)
    {
        var roomData = RoomData.Parse(e.Packet);

        if ((RememberPasswords && roomData.Access == RoomAccess.Password && _passwords.ContainsKey(roomData.Id)) ||
            (DontAskToRingDoorbell && roomData.Access == RoomAccess.Doorbell))
        {
            roomData.IsGroupMember = true;
            e.Packet = new Packet(e.Packet.Header, Client)
                .Write(roomData);
        }
    }

    [InterceptOut(nameof(Outgoing.FlatOpc))]
    private void HandleFlatOpc(InterceptArgs e)
    {
        _lastRequestedRoom = e.Packet.ReadInt();
        string password = e.Packet.ReadString();

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
                e.Packet.ReplaceString(password, 4);
            }
        }
    }

    [InterceptIn(nameof(Incoming.Error))]
    private void HandleError(InterceptArgs e)
    {
        if (!RememberPasswords) return;

        if (e.Packet.ReadInt() == ERROR_INVALID_PW &&
            _lastRequestedRoom != -1 &&
            _passwords.Remove(_lastRequestedRoom))
        {
            Save();
        }
    }
}
