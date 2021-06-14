using System;

using Xabbo.Messages;
using Xabbo.Interceptor;

using Xabbo.Core;
using Xabbo.Core.Game;

namespace b7.Xabbo.Components
{
    public class FlattenRoomComponent : Component
    {
        private readonly RoomManager _roomManager;

        private bool _isActivated = false;
        private Heightmap? _heightmap;

        private Heightmap? _originalHeightmap;
        private FloorPlan? _originalFloorPlan;

        public FlattenRoomComponent(IInterceptor interceptor, RoomManager roomManager)
            : base(interceptor)
        {
            _roomManager = roomManager;
        }

        protected override void OnInitialized(object? sender, InterceptorInitializedEventArgs e)
        {
            base.OnInitialized(sender, e);

            _roomManager.Entering += RoomManager_Entering;
        }

        private void RoomManager_Entering(object? sender, EventArgs e)
        {
            _isActivated = IsActive;
        }

        private float GetOffset(int x, int y)
        {
            if (_originalFloorPlan is null) return 0;

            if (_originalFloorPlan.IsWalkable(x, y))
                return _originalFloorPlan[x, y];
            else
                return 0;
        }

        private double GetOffset((int X, int Y) location) => GetOffset(location.X, location.Y);

        private Tile AdjustTile(Tile tile)
        {
            if (_originalFloorPlan is null) 
                return tile;

            if (_originalFloorPlan.IsWalkable(tile.X, tile.Y))
                return tile - (0, 0, _originalFloorPlan[tile.X, tile.Y]);
            else
                return tile;
        }

        [InterceptIn(nameof(Incoming.StackingHeightmap))]
        private void HandleHeightMap(InterceptArgs e)
        {
            if (!_isActivated) return;

            e.Block();

            // Height map comes before the floor plan so it must be saved
            // and modified after receiving the floor plan
            _originalHeightmap = Heightmap.Parse(e.OriginalPacket);

            e.OriginalPacket.Position = 0;
            _heightmap = Heightmap.Parse(e.OriginalPacket);
        }

        [InterceptIn(nameof(Incoming.FloorHeightmap))]
        private void HandleFloorPlan(InterceptArgs e)
        {
            if (!_isActivated || _heightmap is null) return;

            e.Block();

            _originalFloorPlan = FloorPlan.Parse(e.OriginalPacket);

            e.OriginalPacket.Position = 0;
            FloorPlan floorPlan = FloorPlan.Parse(e.OriginalPacket);
            for (int y = 0; y < floorPlan.Length; y++)
                for (int x = 0; x < floorPlan.Width; x++)
                    if (floorPlan.GetHeight(x, y) >= 0)
                        floorPlan.SetHeight(x, y, 0);

            e.Packet.ReplaceString(floorPlan.ToString(), 5);

            // Modify the heightmap
            for (int y = 0; y < _heightmap.Length; y++)
            {
                for (int x = 0; x < _heightmap.Width; x++)
                {
                    if (_heightmap.IsFree(x, y))
                    {
                        double height = _heightmap.GetHeight(x, y) - GetOffset(x, y);
                        if (height >= 0)
                            _heightmap.SetHeight(x, y, height);
                    }
                }
            }

            Send(In.StackingHeightmap, _heightmap);
            Send(e.Packet);
        }

        [InterceptIn(nameof(Incoming.StackingHeightmapDiff))]
        private void HandleUpdateStackHeight(InterceptArgs e)
        {
            if (!_isActivated) return;

            int n = e.Packet.ReadByte();
            short[] values = new short[n];
            for (int i = 0; i < n; i++)
            {
                int x = e.Packet.ReadByte();
                int y = e.Packet.ReadByte();
                values[i] = e.Packet.ReadShort();
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
                e.Packet.WriteShort(values[i]);
            }
        }

        [InterceptIn(nameof(Incoming.RoomUsers))]
        private void HandleRoomUsers(InterceptArgs e)
        {
            if (!_isActivated) return;

            var entities = Entity.ParseAll(e.Packet);
            foreach (var entity in entities)
                entity.Location = AdjustTile(entity.Location);

            e.Packet = new Packet(e.Client, e.Packet.Header);
            Entity.ComposeAll(e.Packet, entities);
        }

        [InterceptIn(nameof(Incoming.Status))]
        private void HandleEntityUpdate(InterceptArgs e)
        {
            if (!_isActivated) return;

            var updates = EntityStatusUpdate.ParseAll(e.Packet);
            foreach (var update in updates)
            {
                update.Location = AdjustTile(update.Location);

                var movingTo = update.MovingTo;
                if (movingTo.HasValue)
                    update.MovingTo = AdjustTile(movingTo.Value);

                // TODO action height
            }

            e.Packet = new Packet(e.Client, e.Packet.Header);
            EntityStatusUpdate.ComposeAll(e.Packet, updates);
        }

        [InterceptIn(nameof(Incoming.ActiveObjects))]
        private void HandleFloorItems(InterceptArgs e)
        {
            if (!_isActivated) return;

            var floorItems = FloorItem.ParseAll(e.Packet);
            foreach (var item in floorItems)
                item.Location = AdjustTile(item.Location);

            e.Packet = new Packet(e.Client, e.Packet.Header);
            FloorItem.ComposeAll(e.Packet, floorItems);
        }

        [InterceptIn(nameof(Incoming.ActiveObjectAdd))]
        private void HandleAddFloorItem(InterceptArgs e)
        {
            if (!_isActivated) return;

            var floorItem = FloorItem.Parse(e.Packet);
            floorItem.Location = AdjustTile(floorItem.Location);
            e.Packet = Packet.Compose(e.Client, e.Packet.Header, floorItem);
        }

        [InterceptIn(nameof(Incoming.ActiveObjectUpdate))]
        private void HandleUpdateFloorItem(InterceptArgs e)
        {
            if (!_isActivated) return;

            // TODO
        }

        [InterceptIn(nameof(Incoming.QueueMoveUpdate))]
        private void HandleObjectOnRoller(InterceptArgs e)
        {
            if (!_isActivated) return;

            var update = RollerUpdate.Parse(e.Packet);

            float locationOffset = GetOffset(update.LocationX, update.LocationY);
            float targetOffset = GetOffset(update.TargetX, update.TargetY);

            foreach (var objectUpdate in update.ObjectUpdates)
            {
                objectUpdate.LocationZ -= locationOffset;
                objectUpdate.TargetZ -= targetOffset;
            }

            if (update.Type != RollerUpdateType.None)
            {
                update.EntityLocationZ -= locationOffset;
                update.EntityTargetZ -= targetOffset;
            }

            e.Packet = Packet.Compose(e.Client, e.Packet.Header, update);
        }
    }
}
