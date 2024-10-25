using ReactiveUI;
using Xabbo.Abstractions;
using Xabbo.Core;
using Xabbo.Core.GameData;
using Xabbo.Models;

namespace Xabbo.ViewModels;

public abstract class ItemViewModelBase : ViewModelBase
{
    protected readonly FurniInfo? _info;

    public IItem Item { get; private set; }

    public long Id => Item.Id;
    public ItemType Type => Item.Type;
    public int Kind => _info?.Kind ?? 0;
    public string Identifier => Item.Identifier ?? _info?.Identifier ?? "?";
    public string? Variant { get; }

    public string Name { get; }
    public string? Description { get; }

    [Reactive] public bool IsHidden { get; set; }

    public IItemIcon? Icon { get; }

    public ItemViewModelBase(IItem item)
    {
        Item = item;

        if (Extensions.IsInitialized && item.TryGetInfo(out _info))
        {
            if (item.TryGetVariant(out string? variant))
                Variant = variant;

            if (item.TryGetName(out string? name))
                Name = name;
            else
                Name = _info.Identifier;

            if (item.TryGetDescription(out string? desc) && !desc.EndsWith("desc"))
                Description = desc;

            // Hard-coded fix for Origins.
            FurniInfo? iconInfo = _info;
            if (iconInfo.Identifier == "post.it" &&
                !Extensions.TryGetInfo(new WallItem { Identifier = "post_it" }, out iconInfo))
            {
                iconInfo = _info;
            }

            if (iconInfo.Revision > 0)
            {
                Icon = new ItemIcon(iconInfo.Revision, iconInfo.Identifier, variant);
            }
        }
        else
        {
            Name = item.Identifier ?? "?";
        }

        if (item is IFloorItem { Data.IsLimitedRare: true } ltd)
            Name += $" #{ltd.Data.UniqueSerialNumber}";
    }

    public void UpdateItem(IItem item)
    {
        Item = item;
        this.RaisePropertyChanged(nameof(Item));
    }
}
