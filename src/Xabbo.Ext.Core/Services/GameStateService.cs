using Xabbo.Extension;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Ext.Core.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Xabbo.Ext.Services;

public class GameStateService : IGameStateService
{
    private readonly ILogger Log;
    private readonly IExtension _ext;

    public event Action<GameConnectedArgs>? Connected;
    public event Action? Disconnected;

    public Session Session { get; private set; } = Session.None;

    public IGameDataManager GameData { get; }

    public ProfileManager Profile { get; }
    public FriendManager Friends { get; }
    public InventoryManager Inventory { get; }
    public RoomManager Room { get; }
    public TradeManager Trade { get; }

    public GameStateService(IExtension ext,
        IGameDataManager gameData,
        ProfileManager profileManager,
        FriendManager friendManager,
        InventoryManager inventoryManager,
        RoomManager roomManager,
        TradeManager tradeManager,
        ILoggerFactory? loggerFactory = null)
    {
        Log = (ILogger?)loggerFactory?.CreateLogger<GameStateService>() ?? NullLogger.Instance;

        _ext = ext;
        GameData = gameData;

        Profile = profileManager;
        Friends = friendManager;
        Inventory = inventoryManager;
        Room = roomManager;
        Trade = tradeManager;

        ext.Connected += OnConnected;
        ext.Disconnected += OnDisconnected;
    }

    private async void OnConnected(GameConnectedArgs e)
    {
        Session = e.Session;
        Connected?.Invoke(e);

        try
        {
            var hotel = Hotel.FromGameHost(e.Host);
            await GameData.LoadAsync(hotel);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to load game data: {Error}.", ex.Message);
        }
    }

    private void OnDisconnected()
    {
        Session = Session.None;
        Disconnected?.Invoke();
    }
}
