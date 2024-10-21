using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using DynamicData;
using ReactiveUI;
using HanumanInstitute.MvvmDialogs;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Services.Abstractions;
using Xabbo.Controllers;
using Xabbo.Exceptions;
using Xabbo.Utility;

namespace Xabbo.ViewModels;

public class RoomBansViewModel : ViewModelBase
{
    private readonly ILogger Log;
    private readonly IUiContext _uiCtx;
    private readonly IDialogService _dialogService;
    private readonly IOperationManager _operationManager;
    private readonly IExtension _ext;
    private readonly RoomManager _roomManager;
    private readonly RoomModerationController _moderation;

    private readonly SourceCache<RoomBanViewModel, Id> _banCache = new(x => x.Id);
    private readonly ReadOnlyObservableCollection<RoomBanViewModel> _bans;
    public ReadOnlyObservableCollection<RoomBanViewModel> Bans => _bans;

    [Reactive] public string FilterText { get; set; } = "";
    [Reactive] public bool CanLoad { get; set; } = true;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool HasLoaded { get; set; }

    private readonly ObservableAsPropertyHelper<bool> _isUnbanning;
    public bool IsUnbanning => _isUnbanning.Value;

    private readonly ObservableAsPropertyHelper<string> _statusText;
    public string StatusText => _statusText.Value;

    [Reactive] public IList<RoomBanViewModel>? SelectedBans { get; set; }

    public ReactiveCommand<Unit, Unit> LoadBansCmd { get; }
    public ReactiveCommand<IList, Unit> UnbanSelectedUsersCmd { get; }

    public RoomBansViewModel(
        ILoggerFactory loggerFactory,
        IUiContext uiContext,
        IDialogService dialogService,
        IOperationManager operationManager,
        IExtension extension,
        RoomManager roomManager,
        RoomModerationController moderation)
    {
        Log = loggerFactory.CreateLogger<RoomBansViewModel>();
        _uiCtx = uiContext;
        _dialogService = dialogService;
        _operationManager = operationManager;
        _ext = extension;
        _roomManager = roomManager;
        _moderation = moderation;

        _banCache
            .Connect()
            .Filter(this.WhenAnyValue(x => x.FilterText).Select(CreateFilter))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _bans)
            .Subscribe();

        _roomManager.Left += OnLeftRoom;

        _isUnbanning =
            _moderation.WhenAnyValue(
                x => x.CurrentOperation,
                x => x is RoomModerationController.ModerationType.Unban
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.IsUnbanning);

        _statusText =
            _moderation.WhenAnyValue(
                x => x.CurrentOperation,
                x => x.CurrentProgress,
                x => x.TotalProgress,
                (op, current, total) => op is RoomModerationController.ModerationType.Unban
                    ? $"Unbanning users...\n{current} / {total}"
                    : ""
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.StatusText);

        LoadBansCmd = ReactiveCommand.CreateFromTask(
            LoadBansAsync,
            _moderation.WhenAnyValue(
                x => x.CanUnban,
                x => x.CurrentOperation,
                (canUnban, op) => canUnban && op is RoomModerationController.ModerationType.None
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        UnbanSelectedUsersCmd = ReactiveCommand.CreateFromTask<IList>(
            UnbanSelectedUsersAsync,
            Observable.CombineLatest(
                this.WhenAnyValue(
                    x => x.SelectedBans,
                    x => x is { Count: > 0 }
                ),
                _moderation.WhenAnyValue(
                    x => x.CanUnban,
                    x => x.CurrentOperation,
                    (canUnban, op) => canUnban && op is RoomModerationController.ModerationType.None
                ),
                (hasSelection, canUnban) => hasSelection && canUnban
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        _ext.Intercept<UserUnbannedMsg>(unbanned => _banCache.RemoveKey(unbanned.UserId));
    }

    private Func<RoomBanViewModel, bool> CreateFilter(string? filterText)
    {
        if (string.IsNullOrWhiteSpace(filterText))
            return static (vm) => true;

        return (vm) => vm.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase);
    }

    private void OnLeftRoom()
    {
        HasLoaded = false;

        _uiCtx.Invoke(_banCache.Clear);
    }

    private async Task LoadBansAsync()
    {
        if (_roomManager.Room is not { Id: Id currentRoomId })
            return;

        try
        {
            Log.LogDebug("Loading bans...");

            HasLoaded = false;
            IsLoading = true;

            var users = await _ext.RequestAsync(new GetBannedUsersMsg(currentRoomId), timeout: 3000);
            _banCache.Clear();

            Log.LogInformation("Loaded {Count} bans.", users.Length);

            var viewModels = users.Select(x => new RoomBanViewModel(x.Id, x.Name)).ToArray();

            _uiCtx.Invoke(() =>
            {
                foreach (var vm in viewModels)
                    _banCache.AddOrUpdate(vm);
            });

            HasLoaded = true;
        }
        catch (Exception ex)
        {
            _banCache.Clear();
            if (ex is TimeoutException)
            {
                await _dialogService.ShowAsync(
                    "Operation timed out",
                    "Failed to retrieve the ban list.");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task UnbanSelectedUsersAsync(IList selection) => UnbanAsync(selection.OfType<RoomBanViewModel>());

    private async Task UnbanAsync(IEnumerable<RoomBanViewModel> users)
    {
        if (!_roomManager.EnsureInRoom(out var room))
            return;

        try
        {
            await _moderation.UnbanUsersAsync(
                users.Select(vm => new IdName(vm.Id, vm.Name)));
        }
        catch (OperationInProgressException ex)
        {
            await _dialogService.ShowAsync(
                "Operation in progess",
                $"An operation is already in progress: {ex.OperationName}");
        }
        catch { }
    }
}
