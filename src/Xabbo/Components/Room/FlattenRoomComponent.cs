using Xabbo.Messages;
using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Incoming;

namespace Xabbo.Components;

[Intercept(~ClientType.Shockwave)]
public partial class FlattenRoomComponent(IExtension extension, RoomManager roomManager) : Component(extension)
{
    private readonly RoomManager _roomManager = roomManager;

    private bool _isActivated = false;
    private Heightmap? _heightmap;
    private FloorPlan? _originalFloorPlan;

    [Reactive] public bool Enabled { get; set; }

    protected override void OnInitialized(InitializedEventArgs e)
    {
        _roomManager.Entering += RoomManager_Entering;
    }

    private void RoomManager_Entering()
    {
        _isActivated = Enabled;
    }

    private float GetOffset(Point location)
    {
        if (_originalFloorPlan is null) return 0;

        if (_originalFloorPlan.IsWalkable(location.X, location.Y))
            return _originalFloorPlan[location.X, location.Y];
        else
            return 0;
    }

    private float GetOffset(int x, int y) => GetOffset((x, y));

    private FloorItem AdjustTile(FloorItem it)
    {
        it.Location = AdjustTile(it.Location);
        return it;
    }

    private Tile AdjustTile(Tile tile)
    {
        if (_originalFloorPlan is null)
            return tile;

        if (_originalFloorPlan.IsWalkable(tile.X, tile.Y))
            return tile - (0, 0, _originalFloorPlan[tile.X, tile.Y]);
        else
            return tile;
    }

    [InterceptIn("f:"+nameof(In.HeightMap))]
    private void HandleHeightMap(Intercept e)
    {
        if (!_isActivated) return;

        e.Block();

        // Height map comes before the floor plan so it must be saved
        // and modified after receiving the floor plan
        _heightmap = e.Packet.Read<Heightmap>();
    }

    [InterceptIn(nameof(In.FloorHeightMap))]
    private void HandleFloorHeightMap(Intercept e)
    {
        if (!_isActivated || _heightmap is null) return;

        e.Block();

        _originalFloorPlan = e.Packet.Read<FloorPlan>();

        e.Packet.Position = 0;
        FloorPlan floorPlan = e.Packet.Read<FloorPlan>();
        for (int y = 0; y < floorPlan.Size.Y; y++)
            for (int x = 0; x < floorPlan.Size.X; x++)
                if (floorPlan.GetHeight(x, y) >= 0)
                    floorPlan.SetHeight(x, y, 0);

        e.Packet.ReplaceAt(5, floorPlan.ToString());

        // Modify the heightmap
        for (int y = 0; y < _heightmap.Size.Y; y++)
        {
            for (int x = 0; x < _heightmap.Size.X; x++)
            {
                if (_heightmap[x, y].IsFree)
                {
                    float height = _heightmap[x, y].Height - GetOffset(x, y);
                    if (height >= 0)
                        _heightmap[x, y].Height = height;
                }
            }
        }

        Ext.Send(In.HeightMap, _heightmap);
        Ext.Send(e.Packet);
    }

    [InterceptIn(nameof(In.HeightMapUpdate))]
    private void HandleHeightMapUpdate(Intercept e)
    {
        if (!_isActivated) return;

        int n = e.Packet.Read<byte>();
        short[] values = new short[n];
        for (int i = 0; i < n; i++)
        {
            int x = e.Packet.Read<byte>();
            int y = e.Packet.Read<byte>();
            values[i] = e.Packet.Read<short>();
            if ((values[i] & 0xC000) == 0)
            {
                double height = (values[i] & 0x3FFF) / 256.0;
                height -= GetOffset(x, y);
                values[i] = (short)(height * 256);
            }
        }

        e.Packet.Position = 1;
        for (int i = 0; i < n; i++)
        {
            e.Packet.Position += 2;
            e.Packet.Write(values[i]);
        }
    }

    [InterceptIn(nameof(In.Users))]
    private void HandleUsers(Intercept e)
    {
        if (!_isActivated) return;

        var avatars = e.Packet.Read<Avatar[]>();
        foreach (var avatar in avatars)
            avatar.Location = AdjustTile(avatar.Location);

        e.Packet.Clear();
        e.Packet.Write(avatars);
    }

    [InterceptIn(nameof(In.UserUpdate))]
    private void HandleUserUpdate(Intercept e)
    {
        if (!_isActivated) return;

        var updates = e.Packet.Read<AvatarStatus[]>();
        foreach (var update in updates)
        {
            update.Location = AdjustTile(update.Location);

            var movingTo = update.MovingTo;
            if (movingTo.HasValue)
                update.MovingTo = AdjustTile(movingTo.Value);

            if (update.StanceHeight.HasValue)
                update.StanceHeight -= GetOffset(update.Location);
        }

        e.Packet.Clear();
        e.Packet.Write(updates);
    }

    [InterceptIn("f:"+nameof(In.Objects))]
    private void HandleObjects(Intercept e)
    {
        if (!_isActivated) return;

        var floorItems = e.Packet.Read<FloorItemsMsg>();
        foreach (var item in floorItems)
            item.Location = AdjustTile(item.Location);

        e.Packet.Clear();
        e.Packet.Write(floorItems);
    }

    [Intercept]
    private void HandleObjectAdd(Intercept e, FloorItemAddedMsg msg)
    {
        if (!_isActivated) return;

        msg.Item.Location = AdjustTile(msg.Item.Location);
        e.Packet.Clear();
        e.Packet.Write(msg);
    }

    [InterceptIn(nameof(In.ObjectUpdate))]
    private void HandleObjectUpdate(Intercept e)
    {
        if (!_isActivated) return;

        e.Packet.Modify<FloorItem>(AdjustTile);
    }

    [Intercept]
    private void HandleQueueMoveUpdate(Intercept e, SlideObjectBundleMsg msg)
    {
        if (!_isActivated) return;

        var update = msg.Bundle;

        float fromOffset = GetOffset(update.From);
        float toOffset = GetOffset(update.To);

        foreach (var objectUpdate in update.SlideObjects)
        {
            objectUpdate.FromZ -= fromOffset;
            objectUpdate.ToZ -= toOffset;
        }

        if (update.Avatar is not null)
        {
            update.Avatar.FromZ -= fromOffset;
            update.Avatar.ToZ -= toOffset;
        }

        e.Packet.Clear();
        e.Packet.Write(update);
    }
}
