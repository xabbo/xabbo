using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xabbo.Ext.Services;

public interface IOperationManager
{
    bool IsRunning { get; }
    bool IsCancelling { get; }
    Task RunAsync(Func<CancellationToken, Task> task);
    bool Cancel();
}
