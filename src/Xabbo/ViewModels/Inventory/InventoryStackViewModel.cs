using System.Collections;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI;

using Xabbo.Core;

namespace Xabbo.ViewModels;

public sealed class InventoryStackViewModel(IItem item, IEnumerable<IInventoryItem> items)
    : ItemViewModelBase(item), ICollection<IInventoryItem>
{
    private class InventoryItemIdComparer : IEqualityComparer<IInventoryItem>
    {
        public static readonly InventoryItemIdComparer Default = new();
        public bool Equals(IInventoryItem? x, IInventoryItem? y) => x?.ItemId == y?.ItemId;
        public int GetHashCode([DisallowNull] IInventoryItem obj) => obj.ItemId.GetHashCode();
    }

    private readonly HashSet<IInventoryItem> _items = new(items, InventoryItemIdComparer.Default);

    public int Count => IsStickyNote ? _items.Sum(x => int.TryParse(x.Data.Value, out int count) ? count : 0) : _items.Count;

    [Reactive] public int OfferCount { get; set; }

    bool ICollection<IInventoryItem>.IsReadOnly => false;

    public bool IsStickyNote { get; }
        = item.TryGetInfo(out var info) && info.Category == FurniCategory.Sticky;

    public bool Add(IInventoryItem item)
    {
        if (!_items.Add(item))
            return false;

        this.RaisePropertyChanged(nameof(Count));
        return true;
    }

    void ICollection<IInventoryItem>.Add(IInventoryItem item) => Add(item);

    public void Clear()
    {
        _items.Clear();
        this.RaisePropertyChanged(nameof(Count));
    }

    public bool Contains(IInventoryItem item) => _items.Contains(item);

    public void CopyTo(IInventoryItem[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(IInventoryItem item)
    {
        if (!_items.Remove(item))
            return false;

        this.RaisePropertyChanged(nameof(Count));
        return true;
    }

    public IEnumerator<IInventoryItem> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}