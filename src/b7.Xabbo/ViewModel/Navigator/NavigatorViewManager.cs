using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using GalaSoft.MvvmLight.Command;
using MaterialDesignThemes.Wpf;
using Xabbo.Core;
using Xabbo.Core.Tasks;
using Xabbo.Extension;

namespace b7.Xabbo.ViewModel;

public class NavigatorViewManager : ComponentViewModel
{
    private readonly ISnackbarMessageQueue _snackbar;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => Set(ref _searchText, value);
    }

    public ICommand Search { get; set; }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => Set(ref _isLoading, value);
    }

    private readonly ObservableCollection<NavigatorRoomViewModel> _rooms = new();
    public ICollection<NavigatorRoomViewModel> Rooms => _rooms;

    public NavigatorViewManager(
        IExtension extension,
        ISnackbarMessageQueue snackbar)
        : base(extension)
    {
        _snackbar = snackbar;

        Search = new RelayCommand(OnSearch);
    }

    private async void OnSearch()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            NavigatorSearchResults results = await new SearchNavigatorTask(Extension, "query", _searchText)
                .ExecuteAsync(5000, CancellationToken.None);

            _rooms.Clear();
            foreach (var room in results.GetRooms())
            {
                _rooms.Add(new NavigatorRoomViewModel(room));
            }
        }
        catch (Exception ex)
        {
            _snackbar.Enqueue($"Navigator search failed: {ex.Message}");
        }
        finally { IsLoading = false; }
    }
}
