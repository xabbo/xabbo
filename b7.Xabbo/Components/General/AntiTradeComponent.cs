using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Xabbo.Interceptor;
using Xabbo.Messages;

using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;

namespace b7.Xabbo.Components;

public class AntiTradeComponent : Component
{
    private readonly IConfiguration _config;
    private readonly RoomManager _roomManager;
    private readonly ProfileManager _profileManager;
    private readonly TradeManager _tradeManager;

    private bool _isAvailable;
    public bool IsAvailable
    {
        get => _isAvailable;
        set => Set(ref _isAvailable, value);
    }

    public AntiTradeComponent(
        IInterceptor interceptor,
        IConfiguration config,
        ProfileManager profileManager,
        RoomManager roomManager,
        TradeManager tradeManager)
        : base(interceptor)
    {
        _config = config;
        _profileManager = profileManager;
        _roomManager = roomManager;
        _tradeManager = tradeManager;

        _roomManager.Entered += OnRoomEntered;
        _roomManager.RoomDataUpdated += OnRoomDataUpdated;

        IsActive = _config.GetValue("AntiTrade:Active", false);

        Task initialization = Task.Run(InitializeAsync);
    }

    private bool CanTrade()
    {
        IRoomData? roomData = _roomManager.Room?.Data;
        if (roomData is null) return false;

        return (
            roomData.Trading == TradePermissions.Allowed ||
            (roomData.Trading == TradePermissions.RightsHolders && _roomManager.HasRights)
        );
    }

    private void TradeSelf(IRoom? room)
    {
        if (room is null) return;
        if (!room.TryGetUserById(_profileManager.UserData?.Id ?? -1, out IRoomUser? self))
            return;

        if (CanTrade())
        {
            Interceptor.Send(Out.TradeOpen, self.Index);
        }
    }

    private async Task InitializeAsync()
    {
        await _profileManager.GetUserDataAsync();

        IsAvailable = true;
    }

    public override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.RaisePropertyChanged(propertyName);

        UserData? userData = _profileManager.UserData;
        IRoom? room = _roomManager.Room;

        if (userData is null || room is null) return;

        if (propertyName == nameof(IsActive))
        {
            if (IsActive)
            {
                if (_tradeManager.IsTrading)
                {
                    IsActive = false;
                }
                else
                {
                    TradeSelf(room);
                }
            }
            else
            {
                if (_tradeManager.IsTrading &&
                    _tradeManager.Partner is not null &&
                    _tradeManager.Partner.Id == userData.Id)
                {
                    Interceptor.Send(Out.TradeClose);
                }
            }
        }
    }

    private void OnRoomEntered(object? sender, RoomEventArgs e)
    {
        if (!IsActive) return;

        TradeSelf(e.Room);
    }
    private void OnRoomDataUpdated(object? sender, RoomDataEventArgs e)
    {
        if (IsActive && !_tradeManager.IsTrading)
        {
            TradeSelf(_roomManager.Room);
        }
    }

    [InterceptIn(nameof(Incoming.TradeOpen))]
    protected void HandleTradeOpen(InterceptArgs e)
    {
        if (IsActive)
        {
            e.Block();
        }
    }
}
