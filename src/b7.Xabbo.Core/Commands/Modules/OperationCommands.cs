using System;
using System.Threading.Tasks;

using b7.Xabbo.Services;

namespace b7.Xabbo.Commands.Modules;

public class OperationCommands : CommandModule
{
    private readonly IOperationManager _operationManager;

    public OperationCommands(IOperationManager operationManager)
    {
        _operationManager = operationManager;
    }

    [Command("cancel", "c")]
    public Task CancelOperationAsync(CommandArgs args)
    {
        _operationManager.Cancel();

        return Task.CompletedTask;
    }
}
