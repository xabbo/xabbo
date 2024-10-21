using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;
using Avalonia.Controls.Selection;
using FluentAvalonia.UI.Controls;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Services.Abstractions;
using Xabbo.Models;

using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace Xabbo.ViewModels;

public sealed class WardrobePageViewModel : PageViewModel
{
    public override string Header => "Wardrobe";
    public override IconSource? Icon => new SymbolIconSource { Symbol = Symbol.Backpack };

    private readonly IExtension _ext;
    private readonly IWardrobeRepository _repository;
    private readonly IFigureConverterService _figureConverter;
    private readonly IGameStateService _gameState;

    private readonly SourceCache<OutfitViewModel, string> _cache = new(key => key.Figure);

    private readonly ReadOnlyObservableCollection<OutfitViewModel> _outfits;
    public ReadOnlyObservableCollection<OutfitViewModel> Outfits => _outfits;

    public SelectionModel<OutfitViewModel> Selection { get; } = new() { SingleSelect = false };

    public ReactiveCommand<Unit, Unit> AddCurrentFigureCmd { get; }
    public ReactiveCommand<Unit, Unit> RemoveOutfitsCmd { get; }
    public ReactiveCommand<OutfitViewModel, Unit> WearFigureCmd { get; }

    public WardrobePageViewModel(
        IExtension extension,
        IWardrobeRepository repository,
        IFigureConverterService figureConverter,
        IGameStateService gameState)
    {
        _ext = extension;
        _repository = repository;
        _figureConverter = figureConverter;
        _gameState = gameState;

        foreach (var (_, vm) in _cache.KeyValues)
            vm.ModernFigure = vm.Figure;

        _cache
            .Connect()
            .Filter(_gameState.WhenAnyValue(x => x.Session, CreateFilter))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _outfits)
            .Subscribe();

        foreach (var model in _repository.Load())
        {
            var vm = new OutfitViewModel(model);
            _cache.AddOrUpdate(new OutfitViewModel(model));
        }

        AddCurrentFigureCmd = ReactiveCommand.Create(AddCurrentFigure);
        RemoveOutfitsCmd = ReactiveCommand.Create(RemoveOutfits);
        WearFigureCmd = ReactiveCommand.Create<OutfitViewModel>(WearFigure);

        figureConverter.Available += OnFigureConverterAvailable;
    }

    private Func<OutfitViewModel, bool> CreateFilter(Session session) => (vm) => session.Client.Type switch
    {
        ClientType.Origins => vm.IsOrigins,
        ClientType.Flash or ClientType.Unity => !vm.IsOrigins,
        _ => false
    };

    public void AddFigure(Gender gender, string figure)
    {
        FigureModel figureModel = new()
        {
            Gender = gender.ToClientString(),
            FigureString = figure,
            IsOrigins = _gameState.Session.Is(ClientType.Origins)
        };

        if (_repository.Add(figureModel))
        {
            OutfitViewModel vm = new(figureModel);
            UpdateModernFigure(vm);
            _cache.AddOrUpdate(vm);
        }
    }

    private void AddCurrentFigure()
    {
        if (_gameState.Profile.UserData is { } userData)
        {
            AddFigure(userData.Gender, userData.Figure);
        }
    }

    private void WearFigure(OutfitViewModel model)
    {
        _ext.Send(new UpdateAvatarMsg(
            Gender: H.ToGender(model.Gender),
            Figure: model.Figure
        ));
    }

    private void RemoveOutfits()
    {
        var toRemove = Selection
            .SelectedItems
            .OfType<OutfitViewModel>()
            .ToArray();
        _repository.Remove(toRemove.Select(vm => vm.Model));
        _cache.Remove(toRemove);
    }

    private void OnFigureConverterAvailable()
    {
        foreach (var (_, vm) in _cache.KeyValues)
            UpdateModernFigure(vm);
    }

    private void UpdateModernFigure(OutfitViewModel vm)
    {
        if (vm.IsOrigins &&
            vm.ModernFigure is null &&
            _figureConverter.TryConvertToModern(vm.Figure, out Figure? figure))
        {
            vm.ModernFigure = figure.ToString();
        }
    }
}
