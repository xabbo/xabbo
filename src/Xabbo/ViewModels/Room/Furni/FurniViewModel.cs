using Xabbo.Core;

namespace Xabbo.ViewModels;

public class FurniViewModel(IFurni furni) : ItemViewModelBase(furni)
{
    public IFurni Furni => (IFurni)Item;

    public bool IsFloorItem => Furni.Type is ItemType.Floor;
    public bool IsWallItem => Furni.Type is ItemType.Wall;

    // Common properties
    public string Owner => Furni.OwnerName;
    public long OwnerId => Furni.OwnerId;
    public int State => Furni.State;
    public bool Hidden => Furni.IsHidden;

    // Floor item properties
    public int? X => (Furni as IFloorItem)?.X;
    public int? Y => (Furni as IFloorItem)?.Y;
    public double? Z => (Furni as IFloorItem)?.Z;
    public int? Dir => (Furni as IFloorItem)?.Direction;
    public int? LTD => Furni is IFloorItem { Data.IsLimitedRare: true } it ? it.Data.UniqueSerialNumber : null;

    // Wall item properties
    public int? WX => (Furni as IWallItem)?.WX;
    public int? WY => (Furni as IWallItem)?.WY;
    public int? LX => (Furni as IWallItem)?.LX;
    public int? LY => (Furni as IWallItem)?.LY;
    public bool? IsLeft => (Furni as IWallItem)?.Location.Orientation.IsLeft;
    public bool? IsRight => (Furni as IWallItem)?.Location.Orientation.IsRight;

    public string? Data => Furni switch
    {
        IFloorItem floorItem => floorItem.Data.Value,
        IWallItem wallItem => wallItem.Data,
        _ => null
    };

    [Reactive] public int Count { get; set; }
}