using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReactiveUI;

using Xabbo.Services.Abstractions;

namespace Xabbo.Services;

public abstract class ConfigProvider<T> : ReactiveObject, IConfigProvider<T> where T : class, new()
{
    private readonly ILogger Log;
    private readonly JsonTypeInfo<T> _jsonTypeInfo;

    protected abstract string FilePath { get; }
    [Reactive] public T Value { get; private set; }

    public ConfigProvider(
        JsonTypeInfo<T> jsonTypeInfo,
        IHostApplicationLifetime lifetime,
        ILoggerFactory? loggerFactory = null)
    {
        _jsonTypeInfo = jsonTypeInfo;

        Log = (ILogger?)loggerFactory?.CreateLogger<ConfigProvider<T>>() ?? NullLogger.Instance;
        Value = new();

        lifetime.ApplicationStarted.Register(OnApplicationStarted);
        lifetime.ApplicationStopping.Register(OnApplicationStopping);
    }

    private void OnApplicationStarted() => Load();
    private void OnApplicationStopping() => Save();

    public event Action? Loaded;
    public event Action? Saved;

    public void Load()
    {
        try
        {
            FileInfo fi = new(FilePath);
            if (fi.Exists)
            {
                using (DelayChangeNotifications())
                {
                    Log.LogDebug("Loading settings from '{FilePath}'.", FilePath);
                    Value = JsonSerializer.Deserialize<T>(File.ReadAllText(FilePath), _jsonTypeInfo)
                        ?? throw new Exception("Failed to deserialize settings.");
                    Log.LogInformation("Loaded settings.");
                    Loaded?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to load settings.");
        }
    }

    public void Save()
    {
        try
        {
            FileInfo fi = new(FilePath);
            fi.Directory?.Create();
            File.WriteAllText(FilePath, JsonSerializer.Serialize<T>(Value, _jsonTypeInfo));
            Log.LogInformation("Saved settings.");
            Saved?.Invoke();
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to save settings.");
        }
    }
}
