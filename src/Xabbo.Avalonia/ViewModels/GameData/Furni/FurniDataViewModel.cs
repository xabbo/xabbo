using System;
using System.Collections.ObjectModel;
using DynamicData;
using ReactiveUI.Fody.Helpers;

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

    private readonly SourceCache<FurniInfoViewModel, (ItemType, int)> _furniCache = new(x => (x.Type, x.Kind));
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

        _furniCache.Connect().Filter(Filter).Bind(out _furni).Subscribe();

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

    private async void OnGameConnected(GameConnectedArgs e)
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

    private void OnGameDisconnected()
    {
        _uiContext.Invoke(() => _furniCache.Clear());
    }

    private void RefreshList()
    {
        if (!_uiContext.IsSynchronized)
        {
            _uiContext.InvokeAsync(() => RefreshList());
            return;
        }

        _furniCache.Refresh();
    }

    private bool Filter(object o)
    {
        if (o is not FurniInfoViewModel info) return false;

        if (string.IsNullOrWhiteSpace(FilterText)) return true;

        return
            string.IsNullOrWhiteSpace(FilterText) ||
            info.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
            info.Identifier.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
            info.Line.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
            info.Category.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
            (int.TryParse(FilterText, out int i) && info.Kind == i);
    }
}
