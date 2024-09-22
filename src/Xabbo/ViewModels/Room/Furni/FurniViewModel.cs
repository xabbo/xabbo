using System.Reactive.Linq;
using ReactiveUI;

using Xabbo.Core;
using Xabbo.Core.GameData;

namespace Xabbo.ViewModels;

public abstract class ItemViewModelBase : ViewModelBase
{
    protected readonly IItem _item;
    protected readonly FurniInfo? _info;

    public ItemType Type => _info?.Type ?? (ItemType)(-1);
    public int Kind => _info?.Kind ?? 0;
    public string Identifier => _info?.Identifier ?? "?";
    public string Name => _info?.Name ?? _item.Identifier ?? "?";
    public string Description => _info?.Description ?? "";
    public bool HasDescription { get; }

    [Reactive] public bool IsHidden { get; set; }

    [Reactive] public string? IconUrl { get; set; }

    public ItemViewModelBase(IItem item)
    {
        _item = item;
        if (Extensions.IsInitialized)
        {
            if (_item.TryGetInfo(out _info) && _info.Revision > 0)
            {
                string identifier = _info.Identifier.Replace('*', '_');
                if (identifier == "poster" && _item is IWallItem wallItem)
                    identifier += "_" + wallItem.Data;
                IconUrl = $"http://images.habbo.com/dcr/hof_furni/{_info.Revision}/{identifier}_icon.png";
            }
        }

        HasDescription =
            _info is not null &&
            !string.IsNullOrWhiteSpace(_info.Description) &&
            !_info.Description.EndsWith(" desc");
    }
}

public class FurniViewModel(IFurni furni) : ItemViewModelBase(furni)
{
    private readonly IFurni _furni = furni;

    public long Id => _furni.Id;
    public long OwnerId => _furni.OwnerId;
    public string OwnerName => _furni.OwnerName;

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

        _showCount = this.WhenAnyValue(x => x.Count).Select(x => x > 1).ToProperty(this, x => x.ShowCount);
    }
}
