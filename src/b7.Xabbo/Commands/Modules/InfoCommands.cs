using System;
using System.Threading.Tasks;

namespace b7.Xabbo.Commands;

public class InfoCommands : CommandModule
{
    public InfoCommands() { }

    [Command("profile", "prof", "p")]
    public Task ShowProfileAsync(CommandArgs args)
    {
        string name = string.Join(' ', args);
        if (name.StartsWith("id:"))
        {
            name = name[3..].Trim();

            if (long.TryParse(name, out long id))
            {
                return SendAsync(Out.GetExtendedProfile, id, true).AsTask();
            }
            else
            {
                ShowMessage($"Invalid ID specified: '{name}'.");
                return Task.CompletedTask;
            }
        }
        else
        {
            return SendAsync(Out.GetExtendedProfileByUsername, name).AsTask();
        }
    }

    [Command("group", "grp", "g")]
    public Task ShowGroupInfoAsync(CommandArgs args) => SendAsync(Out.GetHabboGroupDetails, long.Parse(args[0]), true).AsTask();
}
