using Microsoft.Extensions.Logging;

namespace Xabbo.Services;

public sealed class GlobalExceptionHandler(ILoggerFactory loggerFactory) : IObserver<Exception>
{
    private readonly ILogger Log = loggerFactory.CreateLogger<GlobalExceptionHandler>();

    public void OnCompleted() { }

    public void OnError(Exception error)
    {
        Log.LogCritical(error, "An unhandled exception occurred.");
    }

    public void OnNext(Exception value)
    {
        Log.LogError(value, "An unhandled exception occurred.");
    }
}