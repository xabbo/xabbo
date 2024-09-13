using Xabbo.Messages.Flash;

namespace Xabbo.Ext.Commands;

[CommandModule]
public sealed class InfoCommands : CommandModule
{
    public InfoCommands() { }

    [Command("profile", "prof", "p")]
    public Task ShowProfileAsync(CommandArgs args)
    {
        string name = string.Join(' ', args);
        if (name.StartsWith("id:"))
        {
            name = name[3..].Trim();

            if (Id.TryParse(name, out Id id))
            {
                Ext.Send(Out.GetExtendedProfile, id, true);
            }
            else
            {
                ShowMessage($"Invalid ID specified: '{name}'.");
            }
        }
        else
        {
            Ext.Send(Out.GetExtendedProfileByName, name);
        }
        return Task.CompletedTask;
    }

    [Command("group", "grp", "g")]
    public Task ShowGroupInfoAsync(CommandArgs args)
    {
        Ext.Send(Out.GetHabboGroupDetails, long.Parse(args[0]), true);
        return Task.CompletedTask;
    }
}
