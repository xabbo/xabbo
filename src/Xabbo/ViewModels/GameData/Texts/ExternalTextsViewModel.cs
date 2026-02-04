using System.Reactive.Linq;

using Xabbo.Core.GameData;
using Xabbo.Services.Abstractions;

namespace Xabbo.ViewModels;

public sealed class ExternalTextsViewModel : KeyValuesViewModel
{
    private readonly IGameDataManager _gameDataManager;
    private readonly IUiContext _uiContext;

    public ExternalTextsViewModel(IGameDataManager gameDataManager, IUiContext uiContext)
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
                if (_gameDataManager.Texts is { } texts)
                    cache.AddOrUpdate(texts.Select(x => new KeyValueViewModel(x.Key, x.Value)));
            });
        });
    }
}