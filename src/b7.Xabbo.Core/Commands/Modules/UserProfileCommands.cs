using System;
using System.Threading.Tasks;

using Xabbo.Messages;
using Xabbo.Interceptor;

namespace b7.Xabbo.Commands;

public class UserProfileCommands : CommandModule
{
    [Command("motto"), RequiredOut(nameof(Outgoing.ChangeAvatarMotto))]
    private Task SetMotto(CommandArgs args)
    {
        Send(Out.ChangeAvatarMotto, string.Join(" ", args));

        return Task.CompletedTask;
    }
}
