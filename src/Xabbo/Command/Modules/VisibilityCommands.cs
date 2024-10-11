using System.Text.RegularExpressions;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Utility;

namespace Xabbo.Command.Modules;

[CommandModule]
public sealed class VisibilityCommands(RoomManager roomManager) : CommandModule
{
    private readonly RoomManager _roomManager = roomManager;

    [Command("show", "hide")]
    public Task HandleShowHide(CommandArgs args)
    {
        if (args is [ string subCommand, .. ])
        {
            switch (subCommand.ToLowerInvariant())
            {
                case "furni" or "f":
                    SetFurniVisibility(string.Join(' ', args.Skip(1)), args.Command.Equals("show"));
                    break;
            }
        }

        return Task.CompletedTask;
    }

    [Command("sf", "hf")]
    public Task HandleShowHideFurni(CommandArgs args)
    {
        SetFurniVisibility(string.Join(' ', args), args.Command.Equals("sf"));
        return Task.CompletedTask;
    }

    private void SetFurniVisibility(string pattern, bool visible)
    {
        IRoom? room = _roomManager.Room;
        if (room is not null)
        {
            Regex regex = StringUtility.CreateWildcardRegex(pattern);
            foreach (IFurni furni in room.Furni)
            {
                if (furni.TryGetName(out string? name) &&
                    regex.IsMatch(name))
                {
                    if (visible)
                        _roomManager.ShowFurni(furni);
                    else
                        _roomManager.HideFurni(furni);
                }
            }
        }
    }
}