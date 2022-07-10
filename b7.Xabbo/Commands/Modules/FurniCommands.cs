using System;
using System.Linq;
using System.Threading.Tasks;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Extensions;

using b7.Xabbo.Services;

namespace b7.Xabbo.Commands
{
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
                string name = string.Join(" ", args.Skip(1));
                if (!string.IsNullOrWhiteSpace(name))
                {
                    foreach (IFurni furni in room.Furni.NamedLike(name))
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
                string name = string.Join(" ", args.Skip(1));
                if (!string.IsNullOrWhiteSpace(name))
                {
                    foreach (IFurni furni in room.Furni.NamedLike(name))
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
                string name = string.Join(" ", args.Skip(1));
                foreach (IFurni furni in room.Furni.NamedLike(name))
                {
                    if (eject == (furni.OwnerId == userData.Id)) continue;
                    _roomManager.Pickup(furni);
                    await Task.Delay(100, ct);
                }
            });
        }
    }
}
