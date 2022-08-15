using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

using Microsoft.Extensions.Hosting;

using Xabbo.Interceptor;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;

using b7.Xabbo.Services;

namespace b7.Xabbo.ViewModel;

public class VisitorsViewManager : ComponentViewModel
{
    private readonly IUiContext _context;
    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private readonly ConcurrentDictionary<long, VisitorViewModel> _visitorMap = new();

    private readonly ObservableCollection<VisitorViewModel> _visitors;

    public ICollectionView Visitors { get; }

    private bool _isAvailable;
    public bool IsAvailable
    {
        get => _isAvailable;
        set => Set(ref _isAvailable, value);
    }

    private string _filterText = string.Empty;
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (Set(ref _filterText, value))
                RefreshList();
        }
    }

#if ENABLE_LOGGING
    private bool _isLogging;
    public bool IsLogging
    {
        get => _isLogging;
        set => Set(ref _isLogging, value);
    }

    const string LOG_DIRECTORY = @"logs\visitors";
    string _currentFilePath;
    DateTime _lastDate;
    long lastRoomId;
#endif

    public VisitorsViewManager(IInterceptor interceptor, IHostApplicationLifetime lifetime,
        IUiContext context,
        ProfileManager profileManager, RoomManager roomManager)
        : base(interceptor)
    {
        _context = context;
        _profileManager = profileManager;
        _roomManager = roomManager;

        _visitors = new ObservableCollection<VisitorViewModel>();

        Visitors = CollectionViewSource.GetDefaultView(_visitors);
        Visitors.SortDescriptions.Add(new SortDescription("Entered", ListSortDirection.Descending));
        Visitors.SortDescriptions.Add(new SortDescription("Index", ListSortDirection.Descending));
        Visitors.Filter = Filter;

        lifetime.ApplicationStarted.Register(() => Task.Run(InitializeAsync));
    }

    private bool Filter(object o)
    {
        if (string.IsNullOrWhiteSpace(_filterText))
            return true;

        if (o is not VisitorViewModel visitor) return false;

        return visitor.Name.ToLower().Contains(FilterText.ToLower());
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _profileManager.GetUserDataAsync();
        }
        catch { return; }

        Interceptor.Disconnected += OnGameDisconnected;

        _roomManager.Left += OnLeftRoom;
        _roomManager.EntitiesAdded += OnEntitiesAdded;
        _roomManager.EntityRemoved += OnEntitiesRemoved;

        IsAvailable = true;
    }

    private void RefreshList()
    {
        if (!_context.IsSynchronized)
        {
            _context.InvokeAsync(() => RefreshList());
            return;
        }

        Visitors.Refresh();
    }

    private void AddVisitors(IEnumerable<VisitorViewModel> newVisitors)
    {
        if (!_context.IsSynchronized)
        {
            _context.InvokeAsync(() => AddVisitors(newVisitors));
            return;
        }

        foreach (var visitor in newVisitors)
            _visitors.Add(visitor);
    }

    private void ClearVisitors()
    {
        if (!_context.IsSynchronized)
        {
            _context.InvokeAsync(() => ClearVisitors());
            return;
        }

        _visitors.Clear();
        _visitorMap.Clear();
    }

#if ENABLE_LOGGING
    private void Log(string text)
    {
        if (!IsLogging || !roomManager.IsInRoom)
            return;

        DateTime today = DateTime.Today;
        if (today != lastDate || currentFilePath == null)
        {
            lastDate = today;
            currentFilePath = Path.Combine(LOG_DIRECTORY, $"{today:yyyy-MM-dd}.txt");
        }

        if (lastRoomId != roomManager.Id)
        {
            lastRoomId = roomManager.Id;

            string roomName = $"(roomid:{roomManager.Id})";
            if (roomManager.Data != null) roomName = $"{H.ReplaceSpecialCharacters(roomManager.Data.Name)} {roomName}";
            text = $"\r\n---------- {roomName} ----------\r\n\r\n" + text;
        }

        File.AppendAllText(currentFilePath, text, Encoding.UTF8);
    }
#endif

    private void OnGameDisconnected(object? sender, EventArgs e) => ClearVisitors();

    private void OnLeftRoom(object? sender, EventArgs e) => ClearVisitors();

    private void OnEntitiesAdded(object? sender, EntitiesEventArgs e)
    {
        if (!_roomManager.IsLoadingRoom && !_roomManager.IsInRoom)
            return;

        bool needsRefresh = false;
        var newLogs = new List<VisitorViewModel>();

        foreach (var user in e.Entities.OfType<IRoomUser>())
        {
            if (_visitorMap.TryGetValue(user.Id, out VisitorViewModel? visitorLog))
            {
                /* User already exists in the dictionary,
                 * so they have re-entered the room */
                visitorLog.Visits++;
                visitorLog.Index = user.Index;
                visitorLog.Entered = DateTime.Now;
                visitorLog.Left = null;
                needsRefresh = true;

#if ENABLE_LOGGING
                Log($"[{DateTime.UtcNow:O}]  In: {user.Name}\r\n");
#endif
            }
            else
            {
                visitorLog = new VisitorViewModel(user.Index, user.Id, user.Name);
                if (_visitorMap.TryAdd(user.Id, visitorLog))
                {
                    /* Entities received when first loading the room were already in the room,
                     * so we don't know when they entered, but we can see what order they
                     * entered the room by their entity index */
                    if (_roomManager.IsLoadingRoom)
                    {
                        // Only set entry time for self
                        if (user.Id == _profileManager.UserData?.Id)
                            visitorLog.Entered = DateTime.Now;
                    }
                    else
                    {
                        visitorLog.Entered = DateTime.Now;

#if ENABLE_LOGGING
                        Log($"[{DateTime.UtcNow:O}]  In: {user.Name}\r\n");
#endif
                    }

                    newLogs.Add(visitorLog);
                }
            }
        }

        if (newLogs.Count > 0)
            _context.InvokeAsync(() => newLogs.ForEach(x => _visitors.Add(x)));
        /* The list gets refreshed when adding new items, only refresh the list 
         * if no new items were added and we need to re-order some items */
        if (newLogs.Count == 0 && needsRefresh)
            RefreshList();
    }


    private void OnEntitiesRemoved(object? sender, EntityEventArgs e)
    {
        if (_visitorMap.TryGetValue(e.Entity.Id, out VisitorViewModel? visitor))
        {
            visitor.Left = DateTime.Now;
#if ENABLE_LOGGING
            if (e.Entity.Id != profileManager.UserData?.Id)
            {
                Log($"[{DateTime.UtcNow:O}] Out: {e.Entity.Name}\r\n");
            }
#endif
        }
    }
}
