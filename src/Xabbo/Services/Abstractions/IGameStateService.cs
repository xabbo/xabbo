using Xabbo.Core.Game;
using Xabbo.Core.GameData;

namespace Xabbo.Services.Abstractions;

public interface IGameStateService
{
    event Action<ConnectedEventArgs>? Connected;
    event Action? Disconnected;

    bool IsConnected { get; }
    Session Session { get; }

    IGameDataManager GameData { get; }

    ProfileManager Profile { get; }
    FriendManager Friends { get; }
    InventoryManager Inventory { get; }
    RoomManager Room { get; }
    TradeManager Trade { get; }
}
