using Microsoft.Extensions.Configuration;
using ReactiveUI;

using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;

namespace Xabbo.Components;

[Intercept(~ClientType.Shockwave)]
public partial class AntiTradeComponent : Component
{
    private readonly IConfigProvider<AppConfig> _config;
    private AppConfig Config => _config.Value;
    private readonly RoomManager _roomManager;
    private readonly ProfileManager _profileManager;
    private readonly TradeManager _tradeManager;

    private bool Enabled
    {
        get => Config.General.AntiTrade;
        set => Config.General.AntiTrade = value;
    }

    public AntiTradeComponent(
        IExtension extension,
        IConfigProvider<AppConfig> config,
        ProfileManager profileManager,
        RoomManager roomManager,
        TradeManager tradeManager)
        : base(extension)
    {
        _config = config;
        _profileManager = profileManager;
        _roomManager = roomManager;
        _tradeManager = tradeManager;

        _roomManager.Entered += OnRoomEntered;
        _roomManager.RoomDataUpdated += OnRoomDataUpdated;

        config
            .ObservableForProperty(config => config.Value.General.AntiTrade)
            .Subscribe(change => OnIsActiveChanged(change.Value));

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
        if (!room.TryGetUserById(_profileManager.UserData?.Id ?? -1, out IUser? self))
            return;

        if (CanTrade())
        {
            Ext.Send(Out.OpenTrading, self.Index);
        }
    }

    private async Task InitializeAsync()
    {
        await _profileManager.GetUserDataAsync();

        IsAvailable = true;
    }

    protected void OnIsActiveChanged(bool isActive)
    {
        UserData? userData = _profileManager.UserData;
        IRoom? room = _roomManager.Room;

        if (userData is null || room is null) return;

        if (isActive)
        {
            if (_tradeManager.IsTrading)
            {
                Enabled = false;
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
                Ext.Send(Out.CloseTrading);
            }
        }
    }

    private void OnRoomEntered(RoomEventArgs e)
    {
        if (!Enabled) return;

        TradeSelf(e.Room);
    }

    private void OnRoomDataUpdated(RoomDataEventArgs e)
    {
        if (Enabled && !_tradeManager.IsTrading)
        {
            TradeSelf(_roomManager.Room);
        }
    }

    [InterceptIn(nameof(In.TradingOpen))]
    protected void HandleTradeOpen(Intercept e)
    {
        if (Enabled)
        {
            e.Block();
        }
    }

    [InterceptIn(nameof(In.NotificationDialog))]
    protected void HandleNotificationDialog(Intercept e)
    {
        if (e.Packet.Read<string>() == "trade.trading_perk")
        {
            Enabled = false;
        }
    }
}
