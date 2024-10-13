using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Xabbo.Core;
using Xabbo.Core.GameData;
using Xabbo.Services.Abstractions;

namespace Xabbo.Services;

public class FigureConverterService : IFigureConverterService
{
    private readonly ILoggerFactory? _loggerFactory;
    private readonly ILogger Log;
    private readonly IGameStateService _gameState;

    private FigureConverter? figureConverter;

    public bool IsAvailable { get; private set; }

    public event Action? Available;

    public FigureConverterService(IGameStateService gameState,
        ILoggerFactory? loggerFactory = null)
    {
        _loggerFactory = loggerFactory;
        Log = (ILogger?)loggerFactory?.CreateLogger<FigureConverterService>() ?? NullLogger.Instance;

        _gameState = gameState;
        _gameState.Disconnected += OnDisconnected;

        _gameState.GameData.Loaded += OnGameDataLoaded;
    }

    private void OnDisconnected()
    {
        IsAvailable = false;
        figureConverter = null;
    }

    private void OnGameDataLoaded()
    {
        Task.Run(InitializeFigureConverter);
    }

    private async Task InitializeFigureConverter()
    {
        if (!_gameState.Session.Is(ClientType.Origins))
            return;

        try
        {
            if (_gameState.GameData.Figure is not { } originsFigureData)
                throw new Exception("Origins figure data is not loaded.");

            Log.LogDebug("Loading modern figure data...");
            GameDataManager modernGameData = new(null, _loggerFactory) { AutoInitCoreExtensions = false };
            await modernGameData.LoadAsync(Hotel.FromIdentifier("us"), [GameDataType.FigureData]);

            if (modernGameData is not { } modernFigureData)
                throw new Exception("Failed to load modern figure data.");

            figureConverter = new FigureConverter(
                modernGameData.Figure!,
                _gameState.GameData.Figure!);

            Log.LogInformation("Initialized figure converter.");

            IsAvailable = true;
            Available?.Invoke();
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to initialize figure converter service: {Error}", ex.Message);
        }
    }

    public bool TryConvertToModern(string originsFigureString, [NotNullWhen(true)] out Figure? figure)
    {
        try
        {
            figure = figureConverter?.ToModern(originsFigureString);
            if (figure is not null)
            {
                if (Log.IsEnabled(LogLevel.Trace))
                {
                    Log.LogTrace("Converted figure '{OriginsFigure}' -> '{ModernFigure}'.",
                        originsFigureString, figure.ToString());
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "Failed to convert Origins figure '{Figure}': {Error}", originsFigureString, ex.Message);
        }

        figure = null;
        return false;
    }
}