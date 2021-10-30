using System;

using GalaSoft.MvvmLight;

using Xabbo.Core;
using Xabbo.Core.GameData;

namespace b7.Xabbo.ViewModel
{
    public class FurniViewModel : ObservableObject
    {
        public FurniInfo Info { get; }

        public string Name { get; }
        public string Description { get; }
        public string Identifier => Info.Identifier;

        public long Id { get; }
        public ItemType Type { get; }
        public long OwnerId { get; }
        public string Owner { get; }

        private bool isHidden;
        public bool IsHidden
        {
            get => isHidden;
            set => Set(ref isHidden, value);
        }

        public string IconUrl { get; }

        public bool IsFloorItem => Type == ItemType.Floor;
        public bool IsWallItem => Type == ItemType.Wall;
        public bool CanStand => Info.CanStandOn;
        public bool CanSit => Info.CanSitOn;
        public bool CanLay => Info.CanLayOn;
        public bool Unwalkable => Info.IsUnwalkable;
        public int XDimension => Info.XDimension;
        public int YDimension => Info.YDimension;
        public FurniCategory Category => Info.Category;
        public string Line => Info.Line;

        public Tile? TileLocation { get; set; }
        public int? X => TileLocation?.X;
        public int? Y => TileLocation?.Y;
        public double? Z => TileLocation?.Z;
        public int? Dir { get; set; }

        public WallLocation? WallLocation { get; set; }
        public int? WX => WallLocation?.WX;
        public int? WY => WallLocation?.WY;
        public int? LX => WallLocation?.LX;
        public int? LY => WallLocation?.LY;
        public char? Wall => WallLocation?.Orientation;

        public string Data { get; set; } = string.Empty;
        public int State { get; set; }
        public long Extra { get; set; }

        public FurniViewModel(FurniInfo info, IFurni item, string name, string description)
        {
            Info = info;

            Name = name;
            Description = description;

            Id = item.Id;
            Type = item.Type;
            OwnerId = item.OwnerId;
            Owner = item.OwnerName;

            string identifier = info.Identifier.Replace('*', '_');
            if (identifier == "poster" && item is IWallItem wallItem)
                identifier += "_" + wallItem.Data;

            IconUrl = $"https://images.habbo.com/dcr/hof_furni/{info.Revision}/{identifier}_icon.png";

            Update(item);
        }

        public void Update(IFurni item)
        {
            if (item is IFloorItem floorItem)
            {
                TileLocation = floorItem.Location;
                Dir = floorItem.Direction;
                Data = floorItem.Data.Value;
                State = floorItem.State;
                Extra = floorItem.Extra;
            }
            else if (item is IWallItem wallItem)
            {
                WallLocation = wallItem.Location;
                Data = wallItem.Data;
                State = wallItem.State;
                Extra = -1;
            }
        }
    }
}
