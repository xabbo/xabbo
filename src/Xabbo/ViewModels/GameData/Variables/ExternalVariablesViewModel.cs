using System.Reactive.Linq;

using Xabbo.Core.GameData;

namespace Xabbo.ViewModels;

public sealed class ExternalVariablesViewModel : KeyValuesViewModel
{
    private readonly IGameDataManager _gameDataManager;

    public ExternalVariablesViewModel(IGameDataManager gameDataManager)
    {
        _gameDataManager = gameDataManager;
        gameDataManager.Loaded += OnGameDataLoaded;
    }

    private void OnGameDataLoaded()
    {
        Cache.Edit(cache => {
            cache.Clear();
            if (_gameDataManager.Variables is { } vars)
                cache.AddOrUpdate(vars.Select(x => new KeyValueViewModel(x.Key, x.Value)));
        });
    }
}