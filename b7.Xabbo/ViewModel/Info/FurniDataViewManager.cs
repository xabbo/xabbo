using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

using GalaSoft.MvvmLight;

using Xabbo.Interceptor;
using Xabbo.Core.GameData;

using b7.Xabbo.Services;


namespace b7.Xabbo.ViewModel;

public class FurniDataViewManager : ObservableObject
{
    private readonly IInterceptor _interceptor;
    private readonly IUiContext _uiContext;
    private readonly IGameDataManager _gameDataManager;

    private readonly ObservableCollection<FurniInfoViewModel> _furni = new();
    public ICollectionView Furni { get; }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => Set(ref _isLoading, value);
    }

    private string _filterText = string.Empty;
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (Set(ref _filterText, value))
            {
                RefreshList();
            }
        }
    }

    private string _errorText = string.Empty;
    public string ErrorText
    {
        get => _errorText;
        set => Set(ref _errorText, value);
    }

    public FurniDataViewManager(
        IInterceptor interceptor,
        IUiContext uiContext,
        IGameDataManager gameDataManager)
    {
        _interceptor = interceptor;
        _uiContext = uiContext;
        _gameDataManager = gameDataManager;

        Furni = CollectionViewSource.GetDefaultView(_furni);
        Furni.Filter = Filter;

        _interceptor.Connected += OnGameConnected;
        _interceptor.Disconnected += OnGameDisconnected;
    }

    private async void OnGameConnected(object? sender, GameConnectedEventArgs e)
    {
        try
        {
            IsLoading = true;

            await _gameDataManager.WaitForLoadAsync(_interceptor.DisconnectToken);

            FurniData? furniData = _gameDataManager.Furni;
            if (furniData is null) return;

            await _uiContext.InvokeAsync(() =>
            {
                foreach (FurniInfo info in furniData)
                {
                    _furni.Add(new FurniInfoViewModel(info));
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

    private void OnGameDisconnected(object? sender, EventArgs e)
    {
        _uiContext.Invoke(() => _furni.Clear());
    }

    private void RefreshList()
    {
        if (!_uiContext.IsSynchronized)
        {
            _uiContext.InvokeAsync(() => RefreshList());
            return;
        }

        Furni.Refresh();
    }

    private bool Filter(object o)
    {
        if (o is not FurniInfoViewModel info) return false;

        if (string.IsNullOrWhiteSpace(_filterText)) return true;

        return
            string.IsNullOrWhiteSpace(_filterText) ||
            info.Name.Contains(_filterText, StringComparison.OrdinalIgnoreCase) ||
            info.Identifier.Contains(_filterText, StringComparison.OrdinalIgnoreCase) ||
            info.Line.Contains(_filterText, StringComparison.OrdinalIgnoreCase) ||
            info.Category.Contains(_filterText, StringComparison.OrdinalIgnoreCase) ||
            (int.TryParse(_filterText, out int i) && info.Kind == i);
    }
}
