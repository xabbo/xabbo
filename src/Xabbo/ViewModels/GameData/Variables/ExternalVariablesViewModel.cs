using System.Reactive.Linq;

using Xabbo.Core.GameData;
using Xabbo.Services.Abstractions;

namespace Xabbo.ViewModels;

public sealed class ExternalVariablesViewModel : KeyValuesViewModel
{
    private readonly IGameDataManager _gameDataManager;
    private readonly IUiContext _uiContext;

    public ExternalVariablesViewModel(IGameDataManager gameDataManager, IUiContext uiContext)
    {
        _gameDataManager = gameDataManager;
        _uiContext = uiContext;
        gameDataManager.Loaded += OnGameDataLoaded;
    }

    private void OnGameDataLoaded()
    {
        _uiContext.Invoke(() =>
        {
            Cache.Edit(cache => {
                cache.Clear();
                if (_gameDataManager.Variables is { } vars)
                    cache.AddOrUpdate(vars.Select(x => new KeyValueViewModel(x.Key, x.Value)));
            });
        });
    }
}