using Xabbo.Core.Messages.Outgoing;
using Xabbo.Messages.Flash;

namespace Xabbo.Command.Modules;

[CommandModule]
public sealed class UserProfileCommands : CommandModule
{
    const short FieldMotto = 6;

    [Command("motto")]
    private async Task SetMotto(CommandArgs args)
    {
        string motto = string.Join(" ", args);
        if (Client is ClientType.Shockwave)
        {
            Ext.Send(new UpdateProfileMsg { Motto = motto });
            await Ext.ReceiveAsync(Xabbo.Messages.Shockwave.In.UPDATEOK);
            ShowMessage("Motto successfully updated.");
        }
        else
        {
            Ext.Send(Out.ChangeMotto, motto);
        }
    }
}
