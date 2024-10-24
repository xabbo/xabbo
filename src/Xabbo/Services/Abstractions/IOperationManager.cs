using System.Diagnostics.CodeAnalysis;

namespace Xabbo.Services.Abstractions;

public interface IOperationManager
{
    bool IsRunning { get; }
    bool IsCancelling { get; }
    Task RunAsync(string operationName, Func<CancellationToken, Task> task, CancellationToken cancellationToken = default);
    Task<T> RunAsync<T>(string operationName, Func<CancellationToken, Task<T>> task, CancellationToken cancellationToken = default);
    bool TryCancelOperation([NotNullWhen(true)] out string? operationName);
}
