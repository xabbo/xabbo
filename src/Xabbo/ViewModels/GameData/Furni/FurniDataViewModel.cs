using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.GameData;
using Xabbo.Services.Abstractions;

namespace Xabbo.ViewModels;

public class FurniDataViewModel : ViewModelBase
{
    private readonly IExtension _extension;
    private readonly IUiContext _uiContext;
    private readonly IGameDataManager _gameDataManager;

    private readonly SourceCache<FurniInfoViewModel, (ItemType, int, string?)> _furniCache = new(x => (x.Type, x.Kind, x.Identifier));
    private readonly ReadOnlyObservableCollection<FurniInfoViewModel> _furni;

    public ReadOnlyObservableCollection<FurniInfoViewModel> Furni => _furni;

    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public string FilterText { get; set; } = "";

    [Reactive] public string ErrorText { get; set; } = "";

    public FurniDataViewModel(
        IExtension extension,
        IUiContext uiContext,
        IGameDataManager gameDataManager)
    {
        _extension = extension;
        _uiContext = uiContext;
        _gameDataManager = gameDataManager;

        _gameDataManager.Loaded += OnGameDataLoaded;

        _furniCache
            .Connect()
            .Filter(this.WhenAnyValue(x => x.FilterText).Select(CreateFilter))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _furni)
            .Subscribe();

        _extension.Connected += OnGameConnected;
        _extension.Disconnected += OnGameDisconnected;
    }

    private void OnGameDataLoaded()
    {
        FurniData? furniData = _gameDataManager.Furni;
        if (furniData is null) return;

        _uiContext.Invoke(() =>
        {
            foreach (FurniInfo info in furniData)
            {
                _furniCache.AddOrUpdate(new FurniInfoViewModel(info));
            }
        });
    }

    private async void OnGameConnected(ConnectedEventArgs e)
    {
        try
        {
            IsLoading = true;

            await _gameDataManager.WaitForLoadAsync(_extension.DisconnectToken);

            FurniData? furniData = _gameDataManager.Furni;
            if (furniData is null) return;

            await _uiContext.InvokeAsync(() =>
            {
                foreach (FurniInfo info in furniData)
                {
                    _furniCache.AddOrUpdate(new FurniInfoViewModel(info));
                }
            });
        }
        catch (Exception ex)
        {
            ErrorText = $"Failed to load furni data: {ex.Message}.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnGameDisconnected() => _furniCache.Clear();

    private Func<FurniInfoViewModel, bool> CreateFilter(string? filterText)
    {
        if (string.IsNullOrWhiteSpace(filterText))
            return static (vm) => true;

        return (vm) => {
            if (string.IsNullOrWhiteSpace(filterText)) return true;

            return
                string.IsNullOrWhiteSpace(filterText) ||
                vm.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
                vm.Identifier.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
                vm.Line.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
                vm.Category.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
                (int.TryParse(filterText, out int i) && vm.Kind == i);
        };
    }
}
