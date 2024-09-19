using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;

namespace Xabbo.Components;

[Intercept]
public partial class AntiHandItemComponent(
    IExtension extension,
    ProfileManager profileManager,
    RoomManager roomManager
)
    : Component(extension)
{
    private readonly ProfileManager _profileManager = profileManager;
    private readonly RoomManager _roomManager = roomManager;

    private readonly SemaphoreSlim semaphore = new(1, 1);
    private DateTime lastUpdate = DateTime.MinValue;

    [Reactive] public bool DropHandItem { get; set; }
    [Reactive] public bool ReturnHandItem { get; set; }
    [Reactive] public bool ShouldMaintainDirection { get; set; }

    [InterceptIn(nameof(In.HandItemReceived))]
    private void HandleHandItemReceived(Intercept e)
    {
        if (ReturnHandItem)
        {
            int index = e.Packet.Read<int>();
            if (_roomManager.Room is not null &&
                _roomManager.Room.TryGetUserByIndex(index, out IUser? user))
            {
                e.Block();
                Ext.Send(Out.PassCarryItem, user.Id);
            }
        }
        else if (DropHandItem)
        {
            e.Block();
            Ext.Send(Out.DropCarryItem);
        }

        if (ShouldMaintainDirection)
        {
            lastUpdate = DateTime.Now;
            Task.Run(TryMaintainDirection);
        }
    }

    private async Task TryMaintainDirection()
    {
        if (await semaphore.WaitAsync(0))
        {
            try
            {
                if (_roomManager.Room is null ||
                    !_roomManager.Room.TryGetUserById(_profileManager.UserData?.Id ?? -1, out IUser? user))
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
                    Ext.Send(Out.LookTo, invX, invY);
                    await Task.Delay(100);
                    Ext.Send(Out.LookTo, x, y);
                }
            }
            finally { semaphore.Release(); }
        }
    }
}
