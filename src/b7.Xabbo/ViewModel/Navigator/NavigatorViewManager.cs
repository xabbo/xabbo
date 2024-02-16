using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using MaterialDesignThemes.Wpf;

using Xabbo.Core;
using Xabbo.Core.Tasks;
using Xabbo.Extension;

namespace b7.Xabbo.ViewModel;

public class NavigatorViewManager : ComponentViewModel
{
    private readonly ISnackbarMessageQueue _snackbar;

    public IReadOnlyList<string> SearchTypes { get; } = new[] {
        "Anything",
        "Room name",
        "Owner",
        "Tag",
        "Group"
    };

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _searchType = "Anything";
    public string SearchType
    {
        get => _searchType;
        set => SetProperty(ref _searchType, value);
    }

    public ICommand Search { get; set; }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
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
            string searchType = SearchType.ToUpperInvariant() switch
            {
                "ANYTHING" => "",
                "ROOM NAME" => "roomname:",
                "OWNER" => "owner:",
                "TAG" => "tag:",
                "GROUP" => "groupname:",
                _ => ""
            };

            NavigatorSearchResults results = await new SearchNavigatorTask(Extension, "query", searchType + _searchText)
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
