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

    private TradePermissions _lastTradePermissions = (TradePermissions)(-1);

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

        config
            .WhenAnyValue(config => config.Value.General.AntiTrade)
            .Subscribe(value => OnIsActiveChanged(value));

        Task initialization = Task.Run(InitializeAsync);

        _roomManager.Entered += OnRoomEntered;
        _roomManager.RoomDataUpdated += OnRoomDataUpdated;
        _tradeManager.Opened += OnTradeOpened;
    }

    private async Task InitializeAsync()
    {
        await _profileManager.GetUserDataAsync();

        IsAvailable = true;
    }

    private bool CanTrade(IRoomData? roomData)
    {
        return (
            !_tradeManager.IsTrading &&
            roomData is not null &&
            (roomData.Trading is TradePermissions.Allowed ||
            (roomData.Trading is TradePermissions.RightsHolders && _roomManager.HasRights))
        );
    }

    private void TradeSelf()
    {
        if (!Session.Is(ClientType.Origins) &&
            _profileManager.UserData is { Id: Id selfId } &&
            _roomManager.EnsureInRoom(out var room) &&
            room.TryGetUserById(selfId, out IUser? self) &&
            CanTrade(room.Data))
        {
            Ext.Send(new TradeUserMsg(self.Index));
        }
    }

    protected void OnIsActiveChanged(bool isActive)
    {
        if (isActive)
        {
            if (_tradeManager.IsTrading)
            {
                Enabled = false;
            }
            else
            {
                TradeSelf();
            }
        }
        else
        {
            if (_tradeManager.IsTrading &&
                _tradeManager.Partner is not null &&
                _tradeManager.Partner.Id == _profileManager.UserData?.Id)
            {
                Ext.Send(new CloseTradeMsg());
            }
        }
    }

    private void OnRoomEntered(RoomEventArgs e)
    {
        _lastTradePermissions = e.Room.Data?.Trading ?? TradePermissions.None;
        if (Enabled) TradeSelf();
    }

    private void OnRoomDataUpdated(RoomDataEventArgs e)
    {
        if (_lastTradePermissions != e.Data.Trading)
        {
            _lastTradePermissions = e.Data.Trading;
            if (Enabled) TradeSelf();
        }
    }

    private void OnTradeOpened(TradeOpenedEventArgs args)
    {
        if (Enabled &&
            Config.General.AntiTradeCloseTrade &&
            args.Self.Id != args.Partner.Id)
        {
            Ext.Send(new CloseTradeMsg());
        }
    }

    [Intercept]
    void HandleTradeOpened(Intercept<TradeOpenedMsg> e)
    {
        if (Enabled) e.Block();
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
