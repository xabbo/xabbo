using Xabbo;
using Xabbo.Messages.Flash;

namespace b7.Xabbo.Commands;

public class UserProfileCommands : CommandModule
{
    const short FieldMotto = 6;

    [Command("motto")]
    private Task SetMotto(CommandArgs args)
    {
        if (Client is ClientType.Shockwave)
        {
            Ext.Send(global::Xabbo.Messages.Shockwave.Out.UPDATE, FieldMotto, string.Join(" ", args));
        }
        else
        {
            Ext.Send(Out.ChangeMotto, string.Join(" ", args));
        }
        return Task.CompletedTask;
    }
}
