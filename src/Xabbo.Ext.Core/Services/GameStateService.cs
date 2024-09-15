using Xabbo.Extension;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;

namespace Xabbo.Ext.Services;

public class GameStateService
{
    private readonly IExtension _ext;

    public IGameDataManager GameData { get; }
    public FurniData? Furni => GameData.Furni;
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
        TradeManager tradeManager)
    {
        _ext = ext;
        GameData = gameData;

        Profile = profileManager;
        Friends = friendManager;
        Inventory = inventoryManager;
        Room = roomManager;
        Trade = tradeManager;

        ext.Connected += OnConnected;
    }

    private async void OnConnected(GameConnectedArgs e)
    {
        try
        {
            var hotel = Hotel.FromGameHost(e.Host);
            await GameData.LoadAsync(hotel);
        }
        catch { }
    }
}
