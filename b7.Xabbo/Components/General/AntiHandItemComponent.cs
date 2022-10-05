using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Extension;
using Xabbo.Messages;

namespace b7.Xabbo.Components;

public class AntiHandItemComponent : Component
{
    private ProfileManager _profileManager;
    private RoomManager _roomManager;

    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    private DateTime lastUpdate = DateTime.MinValue;

    private bool _dropHandItem;
    public bool DropHandItem
    {
        get => _dropHandItem;
        set => Set(ref _dropHandItem, value);
    }

    private bool _returnHandItem;
    public bool ReturnHandItem
    {
        get => _returnHandItem;
        set => Set(ref _returnHandItem, value);
    }

    private bool _maintainDirection;
    public bool MaintainDirection
    {
        get => _maintainDirection;
        set => Set(ref _maintainDirection, value);
    }

    public AntiHandItemComponent(IExtension extension,
        IConfiguration config,
        ProfileManager profileManager,
        RoomManager roomManager)
        : base(extension)
    {
        _profileManager = profileManager;
        _roomManager = roomManager;

        DropHandItem = config.GetValue("AntiHandItem:DropHandItem", false);
        ReturnHandItem = config.GetValue("AntiHandItem:ReturnHandItem", false);
        MaintainDirection = config.GetValue("AntiHandItem:MaintainDirection", false);
    }

    [InterceptIn(nameof(Incoming.HandItemReceived))]
    private async void HandleHandItemReceived(InterceptArgs e)
    {
        if (ReturnHandItem)
        {
            int index = e.Packet.ReadInt();
            if (_roomManager.Room is not null &&
                _roomManager.Room.TryGetUserByIndex(index, out IRoomUser? user))
            {
                e.Block();
                Extension.Send(Out.PassHandItem, (LegacyLong)user.Id);
            }
        }
        else if (DropHandItem)
        {
            e.Block();
            Extension.Send(Out.DropHandItem);
        }

        if (MaintainDirection)
        {
            lastUpdate = DateTime.Now;

            if (await semaphore.WaitAsync(0))
            {
                try
                {
                    if (_roomManager.Room is null ||
                        !_roomManager.Room.TryGetUserById(_profileManager.UserData?.Id ?? -1, out IRoomUser? user))
                    {
                        return;
                    }

                    int dir = user.Direction;

                    while ((DateTime.Now - lastUpdate).TotalSeconds < 1.0)
                    {
                        do { await Task.Delay(100); }
                        while ((DateTime.Now - lastUpdate).TotalSeconds < 1.0);

                        (int x, int y) = H.GetMagicVector(dir);
                        (int invX, int invY) = H.GetMagicVector(dir + 4);

                        await Task.Delay(100);
                        Extension.Send(Out.LookTo, invX, invY);
                        await Task.Delay(100);
                        Extension.Send(Out.LookTo, x, y);
                    }
                }
                finally { semaphore.Release(); }
            }
        }
    }
}
