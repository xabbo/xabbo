using ReactiveUI;

using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Components;

[Intercept]
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
            .WhenAnyValue(config => config.Value.General.AntiTrade)
            .Subscribe(value => OnIsActiveChanged(value));

        Task initialization = Task.Run(InitializeAsync);
    }

    private async Task InitializeAsync()
    {
        await _profileManager.GetUserDataAsync();

        IsAvailable = true;
    }

    private bool CanTrade()
    {
        IRoomData? roomData = _roomManager.Room?.Data;
        if (roomData is null) return false;

        return (
            roomData.Trading is TradePermissions.Allowed ||
            (roomData.Trading is TradePermissions.RightsHolders && _roomManager.HasRights)
        );
    }

    private void TradeSelf(IRoom? room)
    {
        if (Session.Is(ClientType.Origins))
            return;

        if (room is null || !room.TryGetUserById(_profileManager.UserData?.Id ?? -1, out IUser? self))
            return;

        if (CanTrade())
            Ext.Send(new TradeUserMsg(self.Index));
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
                Ext.Send(new CloseTradeMsg());
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
        if (Enabled && !_tradeManager.IsTrading && e.Data.Trading is not TradePermissions.NotAllowed)
            TradeSelf(_roomManager.Room);
    }

    [Intercept]
    void HandleTradeOpened(Intercept<TradeOpenedMsg> e)
    {
        if (Enabled)
        {
            e.Block();
            Ext.Send(new CloseTradeMsg());
        }
    }

    [Intercept]
    void HandleTradeUpdate(Intercept<TradeOffersMsg> e)
    {
        if (Enabled)
        {
            e.Block();
            if (Session.Is(ClientType.Origins))
            {
                if (Config.General.AntiTradeCloseTrade)
                    Ext.Send(new CloseTradeMsg());
            }
        }
    }

    [Intercept]
    void HandleTradeAccepted(Intercept<TradeAcceptedMsg> e)
    {
        if (Enabled) e.Block();
    }

    [Intercept]
    void HandleTradeAwaitingConfirmation(Intercept<TradeAwaitingConfirmationMsg> e)
    {
        if (Enabled) e.Block();
    }

    [Intercept]
    void HandleTradeCompleted(Intercept<TradeCompletedMsg> e)
    {
        if (Enabled) e.Block();
    }

    [Intercept]
    void HandleTradeClosed(Intercept<TradeClosedMsg> e)
    {
        if (Enabled) e.Block();
    }

    [Intercept(ClientType.Modern)]
    [InterceptIn(nameof(In.NotificationDialog))]
    protected void HandleNotificationDialog(Intercept e)
    {
        if (e.Packet.Read<string>() == "trade.trading_perk")
        {
            Enabled = false;
        }
    }
}
