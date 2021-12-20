using System;
using System.Threading.Tasks;

using b7.Xabbo.Components;
using b7.Xabbo.Services;

namespace b7.Xabbo.Commands.Modules
{
    public class OperationCommands : CommandModule
    {
        private readonly IOperationManager _operationManager;
        private readonly XabbotComponent _xabbotComponent;

        public OperationCommands(
            IOperationManager operationManager,
            XabbotComponent xabbotComponent)
        {
            _operationManager = operationManager;
            _xabbotComponent = xabbotComponent;
        }

        [Command("c", "cancel")]
        public Task CancelOperationAsync(CommandArgs args)
        {
            _operationManager.CancelCurrentTask();

            return Task.CompletedTask;
        }
    }
}
