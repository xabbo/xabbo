using System;
using System.Threading;
using System.Threading.Tasks;

namespace b7.Xabbo.Services
{
    public interface IOperationManager
    {
        bool IsTaskRunning { get; }
        Task RunTaskAsync(Func<CancellationToken, Task> task);
        void CancelCurrentTask();
    }
}
