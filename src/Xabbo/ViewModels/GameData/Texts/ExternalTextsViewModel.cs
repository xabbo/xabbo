using System.Reactive.Linq;

using Xabbo.Core.GameData;

namespace Xabbo.ViewModels;

public sealed class ExternalTextsViewModel : KeyValuesViewModel
{
    private readonly IGameDataManager _gameDataManager;

    public ExternalTextsViewModel(IGameDataManager gameDataManager)
    {
        _gameDataManager = gameDataManager;
        gameDataManager.Loaded += OnGameDataLoaded;
    }

    private void OnGameDataLoaded()
    {
        Cache.Edit(cache => {
            cache.Clear();
            if (_gameDataManager.Texts is { } texts)
                cache.AddOrUpdate(texts.Select(x => new KeyValueViewModel(x.Key, x.Value)));
        });
    }
}