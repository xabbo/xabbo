using Xabbo.Messages.Flash;

namespace Xabbo.Command.Modules;

[CommandModule]
public sealed class UserProfileCommands : CommandModule
{
    const short FieldMotto = 6;

    [Command("motto")]
    private async Task SetMotto(CommandArgs args)
    {
        if (Client is ClientType.Shockwave)
        {
            Ext.Send(Xabbo.Messages.Shockwave.Out.UPDATE, FieldMotto, string.Join(" ", args));
            await Ext.ReceiveAsync(Xabbo.Messages.Shockwave.In.UPDATEOK);
            ShowMessage("Motto successfully updated.");
        }
        else
        {
            Ext.Send(Out.ChangeMotto, string.Join(" ", args));
        }
    }
}
