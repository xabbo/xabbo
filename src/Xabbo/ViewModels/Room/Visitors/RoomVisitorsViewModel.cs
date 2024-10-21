using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
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
    private readonly ILogger _logger;
    private readonly IUiContext _context;
    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private readonly ReadOnlyObservableCollection<VisitorViewModel> _visitors;
    public ReadOnlyObservableCollection<VisitorViewModel> Visitors => _visitors;

    private readonly SourceCache<VisitorViewModel, string> _visitorCache = new(x => x.Name);

    [Reactive] public bool IsAvailable { get; set; }
    [Reactive] public string FilterText { get; set; } = "";

    public RoomVisitorsViewModel(
        IHostApplicationLifetime lifetime,
        ILoggerFactory loggerFactory,
        IUiContext context,
        IExtension extension,
        ProfileManager profileManager, RoomManager roomManager)
    {
        _logger = loggerFactory.CreateLogger<RoomVisitorsViewModel>();
        _context = context;
        _ext = extension;
        _profileManager = profileManager;
        _roomManager = roomManager ?? throw new ArgumentNullException(nameof(roomManager));

        var propertyChanges = _visitorCache.Connect()
            .WhenPropertyChanged(x => x.Index)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .Select(_ => Unit.Default);

        _visitorCache.Connect()
            .Filter(this.WhenAnyValue(x => x.FilterText, CreateFilter))
            .ObserveOn(RxApp.MainThreadScheduler)
            .SortAndBind(out _visitors, SortExpressionComparer<VisitorViewModel>.Descending(x => x.Index))
            .Subscribe();

        lifetime.ApplicationStarted.Register(() => Task.Run(InitializeAsync));
    }

    private Func<VisitorViewModel, bool> CreateFilter(string filterText)
    {
        if (string.IsNullOrWhiteSpace(filterText))
            return static (vm) => true;

        return (vm) => vm.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase);
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

    private void ClearVisitors() => _visitorCache.Clear();

    private void OnGameDisconnected() => ClearVisitors();

    private void OnLeftRoom() => ClearVisitors();

    private void OnAvatarsAdded(AvatarsEventArgs e)
    {
        if (!_roomManager.IsLoadingRoom && !_roomManager.IsInRoom)
            return;

        var newLogs = new List<VisitorViewModel>();
        var now = DateTime.Now;

        _visitorCache.Edit(cache => {
            foreach (var user in e.Avatars.OfType<IUser>())
            {
                cache
                    .Lookup(user.Name)
                    .IfHasValue(vm => {
                        vm.Visits++;
                        vm.Index = user.Index;
                        vm.Entered = now;
                    })
                    .Else(() => {
                        var visitorLog = new VisitorViewModel(user.Index, user.Id, user.Name);
                        if (_roomManager.IsLoadingRoom)
                        {
                            if (user.Name == _profileManager.UserData?.Name)
                                visitorLog.Entered = now;
                        }
                        else
                        {
                            visitorLog.Entered = now;
                        }
                        cache.AddOrUpdate(visitorLog);
                    });
            }
            cache.Refresh();
        });
    }

    private void OnAvatarsRemoved(AvatarEventArgs e)
    {
        if (e.Avatar.Type is not AvatarType.User)
            return;

        _visitorCache
            .Lookup(e.Avatar.Name)
            .IfHasValue(vm => {
                vm.Left = DateTime.Now;
            });
    }
}
