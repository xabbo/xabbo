using System.Text.RegularExpressions;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;
using Xabbo.Utility;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Controllers;

namespace Xabbo.Command.Modules;

[CommandModule]
public sealed class FurniCommands(
    IConfigProvider<AppConfig> settingsProvider,
    IOperationManager operationManager,
    IGameDataManager gameData,
    ProfileManager profileManager,
    RoomManager roomManager,
    RoomFurniController furniController) : CommandModule
{
    private readonly IConfigProvider<AppConfig> _settingsProvider = settingsProvider;
    private readonly IOperationManager _operationManager = operationManager;
    private readonly IGameDataManager _gameData = gameData;
    private readonly ProfileManager _profileManager = profileManager;
    private readonly RoomManager _roomManager = roomManager;
    private readonly RoomFurniController _furniController = furniController;

    [Command("pickup", "pick")]
    public Task HandlePickupAsync(CommandArgs args) => PickupFurniAsync(string.Join(" ", args), false, false);

    [Command("eject")]
    public Task HandleEjectAsync(CommandArgs args) => PickupFurniAsync(string.Join(" ", args), false, true);

    private async Task PickupFurniAsync(string pattern, bool matchAll, bool eject)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return;

        IUserData? userData = _profileManager.UserData;
        IRoom? room = _roomManager.Room;

        if (userData is null || room is null)
        {
            ShowMessage(userData is null
                ? "User data is currently unavailable."
                : "Room state is unavailable, please re-enter the room."
            );
            return;
        }

        if (eject && !_roomManager.IsOwner)
        {
            ShowMessage("You must be the room owner to eject furni.");
            return;
        }

        if (Session.Is(ClientType.Origins) && !_roomManager.IsOwner)
        {
            ShowMessage("You must be the room owner to pick up furni.");
            return;
        }

        Regex regex = StringUtility.CreateWildcardRegex(pattern);

        var allFurni = Session.Is(ClientType.Origins)
            ? room.Furni.ToArray()
            : room.Furni.Where(x =>
                eject == (x.OwnerId != userData.Id)
            ).ToArray();

        var matched = matchAll ? allFurni : allFurni.Where(furni =>
            matchAll || (furni.TryGetName(out string? name) && regex.IsMatch(name))
        ).ToArray();

        if (matched.Length == 0)
        {
            ShowMessage($"No furni to {(eject ? "eject" : "pick up")}.");
            return;
        }

        int pickupInterval = Session.Is(ClientType.Origins)
            ? _settingsProvider.Value.Timing.Origins.FurniPickupInterval
            : _settingsProvider.Value.Timing.Modern.FurniPickupInterval;

        int totalDelay = pickupInterval * matched.Length;
        string message = $"Picking up {matched.Length} furni...";
        if (totalDelay >= 2000)
            message += " Use /c to cancel.";
        ShowMessage(message);

        if (eject)
            await _furniController.EjectFurniAsync(matched);
        else
            await _furniController.PickupFurniAsync(matched);
    }
}
