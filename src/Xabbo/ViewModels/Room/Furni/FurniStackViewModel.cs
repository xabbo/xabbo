using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using ReactiveUI;

using Xabbo.Core;

namespace Xabbo.ViewModels;

public class FurniStackViewModel : ItemViewModelBase, ICollection<FurniViewModel>
{
    class FurniViewModelEqualityComparer : IEqualityComparer<FurniViewModel>
    {
        public static readonly FurniViewModelEqualityComparer Default = new();
        public int GetHashCode([DisallowNull] FurniViewModel obj) => (obj.Type, obj.Id).GetHashCode();
        public bool Equals(FurniViewModel? x, FurniViewModel? y) => (x?.Type, x?.Id) == (y?.Type, y?.Id);
    }

    private readonly HashSet<FurniViewModel> _items = new(FurniViewModelEqualityComparer.Default);

    public ItemDescriptor Descriptor { get; }
    public int Count => _items.Count;

    private readonly ObservableAsPropertyHelper<bool> _showCount;
    public bool ShowCount => _showCount.Value;

    [Reactive] public int FilteredCount { get; set; }

    public bool IsReadOnly => false;

    public FurniStackViewModel(ItemDescriptor descriptor) : base(descriptor)
    {
        Descriptor = descriptor;

        _showCount = this
            .WhenAnyValue(x => x.FilteredCount)
            .Select(x => x > 1)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.ShowCount);
    }

    public bool Add(FurniViewModel furni)
    {
        bool added = _items.Add(furni);
        if (added)
        {
            this.RaisePropertyChanged(nameof(Count));
        }
        return added;
    }
    void ICollection<FurniViewModel>.Add(FurniViewModel item) => Add(item);

    public void Clear()
    {
        _items.Clear();
        this.RaisePropertyChanged(nameof(Count));
    }

    public bool Contains(FurniViewModel item) => _items.Contains(item);
    public void CopyTo(FurniViewModel[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(FurniViewModel item)
    {
        bool removed = _items.Remove(item);
        if (removed)
        {
            this.RaisePropertyChanged(nameof(Count));
        }
        return removed;
    }

    public IEnumerator<FurniViewModel> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
