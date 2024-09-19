using Xabbo.Services.Abstractions;

namespace Xabbo.Command.Modules;

[CommandModule]
public sealed class OperationCommands(IOperationManager operationManager) : CommandModule
{
    private readonly IOperationManager _operationManager = operationManager;

    [Command("cancel", "c")]
    public Task CancelOperationAsync(CommandArgs args)
    {
        if (_operationManager.TryCancelOperation(out string? operationName))
        {
            ShowMessage($"Operation canceled. ({operationName})");
        }
        else
        {
            ShowMessage("There is no operation to cancel.");
        }

        return Task.CompletedTask;
    }
}
