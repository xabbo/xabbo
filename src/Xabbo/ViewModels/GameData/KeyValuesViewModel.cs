using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Xabbo.ViewModels;

public abstract class KeyValuesViewModel : ReactiveObject
{
    protected readonly SourceCache<KeyValueViewModel, string> Cache = new(x => x.Key);

    private readonly ReadOnlyObservableCollection<KeyValueViewModel> _entries;
    public ReadOnlyObservableCollection<KeyValueViewModel> Entries => _entries;

    [Reactive] public string FilterText { get; set; } = "";

    protected KeyValuesViewModel()
    {
        Cache
            .Connect()
            .Filter(this.WhenAnyValue(x => x.FilterText).Select(CreateFilter))
            .SortAndBind(out _entries, SortExpressionComparer<KeyValueViewModel>.Ascending(x => x.Key))
            .Subscribe();
    }

    private Func<KeyValueViewModel, bool> CreateFilter(string filterText)
    {
        if (string.IsNullOrWhiteSpace(filterText))
            return static (vm) => true;

        return (vm) =>
            vm.Key.Contains(filterText, StringComparison.CurrentCultureIgnoreCase) ||
            vm.Value.Contains(filterText, StringComparison.CurrentCultureIgnoreCase);
    }
}