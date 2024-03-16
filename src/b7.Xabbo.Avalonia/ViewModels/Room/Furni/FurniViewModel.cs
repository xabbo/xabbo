using System;
using System.Threading.Tasks;

using Avalonia.Media.Imaging;
using ReactiveUI.Fody.Helpers;

using Xabbo.Core;
using Xabbo.Core.Extensions;
using Xabbo.Core.GameData;

using b7.Xabbo.Avalonia.Helpers;
using ReactiveUI;
using System.Reactive.Linq;

namespace b7.Xabbo.Avalonia.ViewModels;

public abstract class ItemViewModelBase : ViewModelBase
{
    protected readonly IItem _item;
    protected readonly FurniInfo _info;

    public ItemType Type => _info.Type;
    public int Kind => _info.Kind;
    public string Identifier => _info.Identifier;
    public string Name => _info.Name;
    public string Description => _info.Description;
    public bool HasDescription { get; }

    private readonly Lazy<Task<Bitmap?>> _icon;
    public Task<Bitmap?> Icon => _icon.Value;

    public ItemViewModelBase(IItem item)
    {
        _item = item;
        _info = _item.GetInfo();
        _icon = new Lazy<Task<Bitmap?>>(LoadIconAsync);

        HasDescription = !string.IsNullOrWhiteSpace(_info.Description) && !_info.Description.EndsWith(" desc");
    }

    protected virtual Task<Bitmap?> LoadIconAsync()
    {
        string identifier = _info.Identifier.Replace('*', '_');
        if (identifier == "poster" && _item is IWallItem wallItem)
            identifier += "_" + wallItem.Data;
        return ImageHelper.LoadFromWeb(new Uri($"http://images.habbo.com/dcr/hof_furni/{_info.Revision}/{identifier}_icon.png"));
    }
}

public class FurniViewModel : ItemViewModelBase
{
    private readonly IFurni _furni;

    public long Id => _furni.Id;
    public long OwnerId => _furni.OwnerId;
    public string OwnerName => _furni.OwnerName;

    [Reactive] public int Count { get; set; }

    public FurniViewModel(IFurni furni) : base(furni)
    {
        _furni = furni;
    }
}

public class FurniStackViewModel : ItemViewModelBase
{
    public static StackDescriptor GetDescriptor(IFurni item)
    {
        string variant = "";
        if (item.GetIdentifier() == "poster" && item is IWallItem wallItem)
            variant = wallItem.Data;
        return new StackDescriptor(item.Type, item.Kind, variant, false, false);
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
