using Xabbo.Ext.Services;

namespace Xabbo.Ext.Commands.Modules;

public class OperationCommands(IOperationManager operationManager) : CommandModule
{
    private readonly IOperationManager _operationManager = operationManager;

    [Command("cancel", "c")]
    public Task CancelOperationAsync(CommandArgs args)
    {
        _operationManager.Cancel();

        return Task.CompletedTask;
    }
}
