using Xabbo.Core.Game;
using Xabbo.Core.GameData;

namespace Xabbo.Ext.Core.Services;

public interface IGameStateService
{
    event Action<GameConnectedArgs>? Connected;
    event Action? Disconnected;

    Session Session { get; }

    IGameDataManager GameData { get; }

    ProfileManager Profile { get; }
    FriendManager Friends { get; }
    InventoryManager Inventory { get; }
    RoomManager Room { get; }
    TradeManager Trade { get; }
}
