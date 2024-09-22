using Avalonia.Logging;
using Microsoft.Extensions.Logging;

namespace Xabbo.Avalonia.Services;

public sealed class AvaloniaLogSink(ILoggerFactory factory) : ILogSink
{
    private readonly ILogger _logger = factory.CreateLogger("Avalonia");

    public bool IsEnabled(LogEventLevel level, string area) => _logger.IsEnabled((LogLevel)level);

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        using (_logger.BeginScope(area))
            _logger.Log((LogLevel)level, messageTemplate);
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        using (_logger.BeginScope(area))
            _logger.Log((LogLevel)level, messageTemplate, propertyValues);
    }
}