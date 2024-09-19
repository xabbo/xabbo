using System;
using System.Reactive;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Xabbo.Extension;
using Xabbo.Messages.Flash;
using Xabbo.Core.Game;
using Xabbo.Services.Abstractions;

namespace Xabbo.ViewModels;

public class RoomBansViewModel : ViewModelBase
{
    private readonly IUiContext _uiCtx;
    private readonly IExtension _ext;
    private readonly RoomManager _roomManager;

    private readonly SourceCache<RoomBanViewModel, long> _banCache = new(x => x.Id);

    [Reactive] public string FilterText { get; set; } = "";
    [Reactive] public bool CanLoad { get; set; } = true;
    [Reactive] public bool IsLoading { get; set; }

    public ReactiveCommand<Unit, Unit> UnbanCommand { get; }
    public ReactiveCommand<Unit, Unit> LoadBansCommand { get; }

    public RoomBansViewModel(IUiContext uiContext, IExtension extension, RoomManager roomManager)
    {
        _uiCtx = uiContext;
        _ext = extension;
        _roomManager = roomManager;

        _roomManager.Left += OnLeftRoom;

        UnbanCommand = ReactiveCommand.CreateFromTask(UnbanAsync);
        LoadBansCommand = ReactiveCommand.CreateFromTask(LoadBansAsync);
    }

    private Task UnbanAsync()
    {
        return Task.CompletedTask;
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
            IsLoading = true;

            var receiver = _ext.ReceiveAsync(In.BannedUsersFromRoom, timeout: 3000, block: true);
            _ext.Send(Out.GetBannedUsersFromRoom, currentRoomId);
            var packet = await receiver;
            long roomId = packet.Read<Id>();
            if (roomId != currentRoomId)
            {
                throw new Exception("Room ID mismatch");
            }

            int n = packet.Read<Length>();
            var viewModels = new RoomBanViewModel[n];
            for (int i = 0; i < n; i++)
            {
                var (id, name) = packet.Read<Id, string>();
                viewModels[i] = new RoomBanViewModel(id, name);
            }

            _uiCtx.Invoke(() =>
            {
                foreach (var vm in viewModels)
                {
                    _banCache.AddOrUpdate(vm);
                }
            });
        }
        catch { }
        finally
        {
            IsLoading = false;
        }
    }
}
