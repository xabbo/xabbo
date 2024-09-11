using b7.Xabbo.Services;

namespace b7.Xabbo.Commands.Modules;

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
