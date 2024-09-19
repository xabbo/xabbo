using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Xabbo.Models;
using Xabbo.Serialization;
using Xabbo.Services.Abstractions;

namespace Xabbo.Services;

public sealed class JsonWardrobeRepository(ILoggerFactory? loggerFactory = null) : IWardrobeRepository
{
    private readonly ILogger Log = (ILogger?)loggerFactory?.CreateLogger<JsonWardrobeRepository>() ?? NullLogger.Instance;
    private readonly string _filePath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "xabbo",
        "wardrobe.json"
    );
    private readonly ConcurrentDictionary<string, FigureModel> _models = [];
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);
    private bool _isLoaded;

    public void Initialize() { }

    public IEnumerable<FigureModel> Load()
    {
        if (!_loadSemaphore.Wait(0))
            throw new InvalidOperationException("The repository has already been loaded.");

        FileInfo fi = new(_filePath);

        if (File.Exists(_filePath))
        {
            var figures = JsonSerializer.Deserialize<List<FigureModel>>(
                File.ReadAllText(_filePath),
                JsonSourceGenerationContext.Default.ListFigureModel);

            if (figures is not null)
            {
                foreach (var figure in figures)
                    _models.TryAdd(figure.FigureString, figure);

                Log.LogInformation("Loaded {Count} outfits.", _models.Count);
            }
        }
        else
        {
            fi.Directory?.Create();
        }

        _isLoaded = true;
        return _models.Values;
    }

    private void Save()
    {
        if (!_isLoaded) return;

        try
        {
            string json = JsonSerializer.Serialize(
                _models.Values, JsonSourceGenerationContext.Default.IEnumerableFigureModel);

            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to save wardrobe repository: {Error}", ex.Message);
        }
    }

    public bool Add(FigureModel figure)
    {
        if (!_models.TryAdd(figure.FigureString, figure))
            return false;

        Save();
        return true;
    }

    public int Add(IEnumerable<FigureModel> figures)
    {
        int added = 0;

        foreach (var figure in figures)
        {
            if (_models.TryAdd(figure.FigureString, figure))
                added++;
        }

        if (added > 0)
            Save();

        return added;
    }

    public bool Remove(FigureModel figure)
    {
        if (!_models.TryRemove(figure.FigureString, out _))
            return false;

        Save();
        return true;
    }

    public int Remove(IEnumerable<FigureModel> figures)
    {
        int removed = 0;

        foreach (var figure in figures)
        {
            if (_models.TryRemove(figure.FigureString, out _))
                removed++;
        }

        if (removed > 0)
            Save();

        return removed;
    }

    public bool Update(FigureModel figure)
    {
        if (!_models.TryGetValue(figure.FigureString, out var existing) ||
            !_models.TryUpdate(figure.FigureString, figure, existing))
        {
            return false;
        }

        Save();
        return true;
    }

    public int Update(IEnumerable<FigureModel> figures)
    {
        int updated = 0;

        foreach (var figure in figures)
        {
            if (_models.TryGetValue(figure.FigureString, out var existing) &&
                _models.TryUpdate(figure.FigureString, figure, existing))
            {
                updated++;
            }
        }

        if (updated > 0)
            Save();

        return updated;
    }
}