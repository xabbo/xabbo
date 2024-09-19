using System.Text.RegularExpressions;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;

using Xabbo.Ext.Services;
using Xabbo.Ext.Util;

namespace Xabbo.Ext.Commands;

[CommandModule]
public sealed class FurniCommands(
    IOperationManager operationManager,
    IGameDataManager gameData,
    ProfileManager profileManager,
    RoomManager roomManager) : CommandModule
{
    private readonly IOperationManager _operationManager = operationManager;
    private readonly IGameDataManager _gameData = gameData;
    private readonly ProfileManager _profileManager = profileManager;
    private readonly RoomManager _roomManager = roomManager;

    [Command("furni", "f")]
    public async Task OnExecuteAsync(CommandArgs args)
    {
        if (args.Count < 1) return;
        string subCommand = args[0].ToLower();

        switch (subCommand)
        {
            case "s":
            case "show":
                await ShowFurniAsync(args); break;
            case "h":
            case "hide":
                await HideFurniAsync(args); break;
            case "p":
            case "pick":
            case "pickup":
                await PickupFurniAsync(args, false); break;
            case "e":
            case "eject":
                if (Session.IsOrigins)
                {
                    ShowMessage("Origins does not support ejecting furni.");
                    return;
                }
                await PickupFurniAsync(args, true); break;
        }
    }

    private Task ShowFurniAsync(CommandArgs args)
    {
        IRoom? room = _roomManager.Room;
        if (room is not null)
        {
            string pattern = string.Join(' ', args.Skip(1));
            Regex regex = StringUtil.CreateWildcardRegex(pattern);
            foreach (IFurni furni in room.Furni)
            {
                if (furni.TryGetName(out string? name) &&
                    regex.IsMatch(name))
                {
                    _roomManager.ShowFurni(furni);
                }
            }
        }

        return Task.CompletedTask;
    }

    private Task HideFurniAsync(CommandArgs args)
    {
        IRoom? room = _roomManager.Room;
        if (room is not null)
        {
            string pattern = string.Join(" ", args.Skip(1));
            Regex regex = StringUtil.CreateWildcardRegex(pattern);
            foreach (IFurni furni in room.Furni)
            {
                if (furni.TryGetName(out string? name) &&
                    regex.IsMatch(name))
                {
                    _roomManager.HideFurni(furni);
                }
            }
        }

        return Task.CompletedTask;
    }

    private async Task PickupFurniAsync(CommandArgs args, bool eject)
    {
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

        if (Session.IsOrigins && _roomManager.RightsLevel < 4)
        {
            ShowMessage("You must be the room owner to pick up furni.");
            return;
        }

        await _operationManager.RunAsync($"{(eject ? "eject" : "pickup")} furni", async ct =>
        {
            string pattern = string.Join(" ", args.Skip(1));
            if (string.IsNullOrWhiteSpace(pattern)) return;

            bool all = pattern.Equals("all", StringComparison.OrdinalIgnoreCase);
            Regex regex = StringUtil.CreateWildcardRegex(pattern);

            var allFurni = Session.IsOrigins
                ? room.Furni.ToArray()
                : room.Furni.Where(x =>
                    eject == (x.OwnerId != userData.Id)
                ).ToArray();

            var matched = all ? allFurni : allFurni.Where(furni =>
                furni.TryGetName(out string? name) &&
                (all || regex.IsMatch(name))
            ).ToArray();

            if (matched.Length == 0)
            {
                ShowMessage($"No furni to {(eject ? "eject" : "pick up")}.");
                return;
            }

            if (matched.Length == allFurni.Length && !all)
            {
                ShowMessage($"[Warning] Pattern matched all furni. Use '/{args.Command} {args[0]} all' to {(eject ? "eject" : "pick up")} all furni.");
                return;
            }

            int pickupInterval = Session.IsOrigins ? 750 : 100;
            int totalDelay = pickupInterval * matched.Length;
            string message = $"Picking up {matched.Length} furni...";
            if (totalDelay >= 2500)
                message += " Use /c to cancel.";
            ShowMessage(message);

            foreach (var furni in matched)
            {
                if (!Session.IsOrigins && eject == (furni.OwnerId == userData.Id)) continue;
                _roomManager.Pickup(furni);
                await Task.Delay(pickupInterval, ct);
            }
        });
    }
}
