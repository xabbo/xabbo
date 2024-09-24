using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive;
using DynamicData;
using ReactiveUI;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;

using Xabbo.Extension;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing.Modern;
using Xabbo.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Xabbo.ViewModels;

public class RoomBansViewModel : ViewModelBase
{
    private readonly ILogger Log;
    private readonly IUiContext _uiCtx;
    private readonly IDialogService _dialogService;
    private readonly IOperationManager _operationManager;
    private readonly IExtension _ext;
    private readonly RoomManager _roomManager;

    private readonly SourceCache<RoomBanViewModel, Id> _banCache = new(x => x.Id);
    private readonly ReadOnlyObservableCollection<RoomBanViewModel> _bans;
    public ReadOnlyObservableCollection<RoomBanViewModel> Bans => _bans;

    [Reactive] public string FilterText { get; set; } = "";
    [Reactive] public bool CanLoad { get; set; } = true;
    [Reactive] public bool IsLoading { get; set; }

    public ReactiveCommand<Unit, Unit> LoadBansCmd { get; }
    public ReactiveCommand<IList, Unit> UnbanSelectedUsersCmd { get; }

    public RoomBansViewModel(
        ILoggerFactory loggerFactory,
        IUiContext uiContext,
        IDialogService dialogService,
        IOperationManager operationManager,
        IExtension extension,
        RoomManager roomManager)
    {
        Log = loggerFactory.CreateLogger<RoomBansViewModel>();
        _uiCtx = uiContext;
        _dialogService = dialogService;
        _operationManager = operationManager;
        _ext = extension;
        _roomManager = roomManager;

        _banCache
            .Connect()
            .Filter(FilterBans)
            .Bind(out _bans)
            .Subscribe();

        _roomManager.Left += OnLeftRoom;

        LoadBansCmd = ReactiveCommand.CreateFromTask(LoadBansAsync);
        UnbanSelectedUsersCmd = ReactiveCommand.CreateFromTask<IList>(UnbanSelectedUsersAsync);
    }

    private bool FilterBans(RoomBanViewModel ban)
    {
        return true;
    }

    private void OnLeftRoom()
    {
        _uiCtx.Invoke(_banCache.Clear);
    }

    private async Task LoadBansAsync()
    {
        long currentRoomId = _roomManager.CurrentRoomId;
        if (currentRoomId <= 0) return;

        try
        {
            Log.LogDebug("Loading bans...");

            IsLoading = true;

            var msg = await _ext.RequestAsync(new GetBannedUsersMsg(currentRoomId), timeout: 3000);
            _banCache.Clear();

            if (msg.RoomId != currentRoomId)
            {
                throw new Exception("Room ID mismatch");
            }

            Log.LogInformation("Loaded {Count} bans.", msg.Users.Count);

            var viewModels = msg.Users.Select(x => new RoomBanViewModel(x.Id, x.Name)).ToArray();

            _uiCtx.Invoke(() =>
            {
                foreach (var vm in viewModels)
                    _banCache.AddOrUpdate(vm);
            });
        }
        catch
        {
            _banCache.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task UnbanSelectedUsersAsync(IList selection) => UnbanAsync(selection.OfType<RoomBanViewModel>());

    private async Task UnbanAsync(IEnumerable<RoomBanViewModel> users)
    {
        if (!_roomManager.IsInRoom)
            return;

        Id roomId = _roomManager.CurrentRoomId;
        if (roomId <= 0) return;

        try
        {
            var array = users.ToArray();
            if (array.Length == 0) return;

            Log.LogDebug("Unbanning {Count} users.", array.Length);

            await _operationManager.RunAsync("unban users", async cancellationToken => {
                for (int i = 0; i < array.Length; i++)
                {
                    if (i > 0)
                        await Task.Delay(500, cancellationToken);
                    Log.LogTrace("Unbanning user '{Name}'.", array[i].Name);
                    await _ext.RequestAsync(new UnbanUserMsg(roomId, array[i].Id), cancellationToken: cancellationToken);
                    _banCache.RemoveKey(array[i].Id);
                }
            });
        }
        catch (Exception ex)
        {
            await _dialogService.ShowContentDialogAsync(
                _dialogService.CreateViewModel<MainViewModel>(),
                new ContentDialogSettings
                {
                    Title = "Error",
                    Content = $"Failed to retrieve ban list: {ex.Message}",
                    PrimaryButtonText = "OK"
                }
            );
        }
        finally
        {

        }
    }
}
