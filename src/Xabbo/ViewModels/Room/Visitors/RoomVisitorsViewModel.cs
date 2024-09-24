using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Services.Abstractions;

namespace Xabbo.ViewModels;

public class RoomVisitorsViewModel : ViewModelBase
{
    private readonly IExtension _ext;
    private readonly IUiContext _context;
    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private readonly ConcurrentDictionary<string, VisitorViewModel> _visitorMap = new();

    private readonly ReadOnlyObservableCollection<VisitorViewModel> _visitors;
    public ReadOnlyObservableCollection<VisitorViewModel> Visitors => _visitors;

    private readonly SourceCache<VisitorViewModel, string> _visitorCache = new(x => x.Name);

    [Reactive] public bool IsAvailable { get; set; }
    [Reactive] public string FilterText { get; set; } = ""; // RefreshList on set

#if ENABLE_LOGGING
    private bool _isLogging;
    public bool IsLogging
    {
        get => _isLogging;
        set => SetProperty(ref _isLogging, value);
    }

    const string LOG_DIRECTORY = "logs/visitors";
    string _currentFilePath;
    DateTime _lastDate;
    long lastRoomId;
#endif

    public RoomVisitorsViewModel(
        IUiContext context,
        IExtension extension,
        ProfileManager profileManager, RoomManager roomManager)
    {
        _context = context;
        _ext = extension;
        _profileManager = profileManager;
        _roomManager = roomManager ?? throw new ArgumentNullException(nameof(roomManager));

        _visitorCache.Connect()
            .Filter(Filter)
            .Sort(SortExpressionComparer<VisitorViewModel>
                .Descending(x => x.Entered ?? DateTime.MinValue)
                .ThenByDescending(x => x.Index))
            .Bind(out _visitors)
            .Subscribe();

        this.WhenAnyValue(x => x.FilterText)
            .Subscribe(x => _visitorCache.Refresh());

        // lifetime.ApplicationStarted.Register(() => Task.Run(InitializeAsync));
        Task.Run(InitializeAsync);
    }

    private bool Filter(VisitorViewModel visitor)
    {
        if (string.IsNullOrWhiteSpace(FilterText))
            return true;

        return visitor.Name.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase);
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _profileManager.GetUserDataAsync();
        }
        catch { return; }

        _ext.Disconnected += OnGameDisconnected;

        _roomManager.Left += OnLeftRoom;
        _roomManager.AvatarsAdded += OnAvatarsAdded;
        _roomManager.AvatarRemoved += OnAvatarsRemoved;

        IsAvailable = true;
    }

    private void RefreshList()
    {
        /*if (!_context.IsSynchronized)
        {
            _context.InvokeAsync(() => RefreshList());
            return;
        }*/

        _visitorCache.Refresh();
    }

    private void AddVisitors(IEnumerable<VisitorViewModel> newVisitors)
    {
        if (!_context.IsSynchronized)
        {
            _context.InvokeAsync(() => AddVisitors(newVisitors));
            return;
        }

        foreach (var visitor in newVisitors)
            _visitorCache.AddOrUpdate(visitor);
    }

    private void ClearVisitors()
    {
        if (!_context.IsSynchronized)
        {
            _context.InvokeAsync(() => ClearVisitors());
            return;
        }

        _visitorCache.Clear();
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

    private void OnGameDisconnected() => ClearVisitors();

    private void OnLeftRoom() => ClearVisitors();

    private void OnAvatarsAdded(AvatarsEventArgs e)
    {
        if (!_roomManager.IsLoadingRoom && !_roomManager.IsInRoom)
            return;

        bool needsRefresh = false;
        var newLogs = new List<VisitorViewModel>();

        foreach (var user in e.Avatars.OfType<IUser>())
        {
            if (_visitorMap.TryGetValue(user.Name, out VisitorViewModel? visitorLog))
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
                if (_visitorMap.TryAdd(user.Name, visitorLog))
                {
                    /* Avatars received when first loading the room were already in the room,
                     * so we don't know when they entered, but we can see what order they
                     * entered the room by their avatar index */
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
            _context.InvokeAsync(() => newLogs.ForEach(x => _visitorCache.AddOrUpdate(x)));
        /* The list gets refreshed when adding new items, only refresh the list
         * if no new items were added and we need to re-order some items */
        if (newLogs.Count == 0 && needsRefresh)
            RefreshList();
    }


    private void OnAvatarsRemoved(AvatarEventArgs e)
    {
        if (_visitorMap.TryGetValue(e.Avatar.Name, out VisitorViewModel? visitor))
        {
            visitor.Left = DateTime.Now;
#if ENABLE_LOGGING
            if (e.Avatar.Id != profileManager.UserData?.Id)
            {
                Log($"[{DateTime.UtcNow:O}] Out: {e.Avatar.Name}\r\n");
            }
#endif
        }
    }
}
