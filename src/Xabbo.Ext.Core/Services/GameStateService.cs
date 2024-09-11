using Xabbo;
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

    public GameStateService(IExtension ext, IGameDataManager gameData)
    {
        _ext = ext;
        GameData = gameData;

        Profile = new ProfileManager(ext);
        Friends = new FriendManager(ext);
        Inventory = new InventoryManager(ext);
        Room = new RoomManager(ext);
        Trade = new TradeManager(ext, Profile, Room);

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
