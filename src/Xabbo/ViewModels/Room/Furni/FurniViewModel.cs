using System.Reactive.Linq;
using ReactiveUI;

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

public class FurniStackViewModel : ItemViewModelBase
{
    public static StackDescriptor GetDescriptor(IFurni item)
    {
        if (!Extensions.IsInitialized)
            return new StackDescriptor(item.Type, item.Kind, item.Identifier, "", false, false);

        item.TryGetIdentifier(out string? identifier);
        string variant = "";
        if (identifier == "poster" && item is IWallItem wallItem)
            variant = wallItem.Data;
        return new StackDescriptor(item.Type, item.Kind, identifier, variant, false, false);
    }

    public StackDescriptor Descriptor { get; }
    [Reactive] public int Count { get; set; } = 1;

    private readonly ObservableAsPropertyHelper<bool> _showCount;
    public bool ShowCount => _showCount.Value;

    public FurniStackViewModel(IFurni item) : base(item)
    {
        Descriptor = GetDescriptor(item);

        _showCount = this
            .WhenAnyValue(x => x.Count)
            .Select(x => x > 1)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.ShowCount);
    }
}
