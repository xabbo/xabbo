using System.Diagnostics.CodeAnalysis;

namespace Xabbo.Ext.Services;

public interface IOperationManager
{
    bool IsRunning { get; }
    bool IsCancelling { get; }
    Task RunAsync(string operationName, Func<CancellationToken, Task> task);
    bool TryCancelOperation([NotNullWhen(true)] out string? operationName);
}
