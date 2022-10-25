using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Extensions;

using b7.Xabbo.Services;
using b7.Xabbo.Util;

namespace b7.Xabbo.Commands;

public class FurniCommands : CommandModule
{
    private readonly IOperationManager _operationManager;
    private readonly IGameDataManager _gameData;
    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    public FurniCommands(
        IOperationManager operationManager,
        IGameDataManager gameData,
        ProfileManager profileManager,
        RoomManager roomManager)
    {
        _operationManager = operationManager;
        _gameData = gameData;
        _profileManager = profileManager;
        _roomManager = roomManager;
    }

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

        if (userData is null || room is null) return;

        await _operationManager.RunAsync(async ct =>
        {
            string pattern = string.Join(" ", args.Skip(1));
            if (string.IsNullOrWhiteSpace(pattern)) return;

            bool matchAll = pattern.Equals("all", StringComparison.OrdinalIgnoreCase);
            Regex regex = StringUtil.CreateWildcardRegex(pattern);

            var allFurni = room.Furni.Where(x => eject == (x.OwnerId != userData.Id)).ToArray();
            if (allFurni.Length == 0)
            {
                ShowMessage($"No furni to {(eject ? "eject" : "pick up")}.");
                return;
            }

            var matched = allFurni.Where(furni =>
                furni.TryGetName(out string? name) &&
                (matchAll || regex.IsMatch(name))
            ).ToArray();

            if (matched.Length == allFurni.Length && !matchAll)
            {
                ShowMessage($"[Warning] Pattern matched all furni. Use '/{args.Command} {args[0]} all' to {(eject ? "eject" : "pick up")} all furni.");
                return;
            }

            int totalDelay = 100 * matched.Length;
            string message = $"Picking up {matched.Length} furni...";
            if ((100 * matched.Length) > 3000)
                message += " Use /c to cancel.";
            ShowMessage(message);
            foreach (var furni in matched)
            {
                if (eject == (furni.OwnerId == userData.Id)) continue;
                _roomManager.Pickup(furni);
                await Task.Delay(100, ct);
            }
        });
    }
}
