using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls.Selection;

using DynamicData;

using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Xabbo.Core;
using Xabbo.Ext.Core.Services;
using Xabbo.Ext.Model;
using Xabbo.Ext.Services;
using Xabbo.Extension;

using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

using Modern = Xabbo.Core.Messages.Outgoing.Modern;

namespace Xabbo.Ext.Avalonia.ViewModels;

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

    private void AddCurrentFigure()
    {
        if (_gameState.Profile.UserData is { } userData)
        {
            FigureModel figureModel = new()
            {
                FigureString = userData.Figure,
                Gender = userData.Gender.ToClientString(),
                IsOrigins = _gameState.Session.IsOrigins
            };

            if (_repository.Add(figureModel))
            {
                OutfitViewModel vm = new(figureModel);
                UpdateModernFigure(vm);
                _cache.AddOrUpdate(vm);
            }
        }
    }

    private void WearFigure(OutfitViewModel model)
    {
        _ext.Send(new Modern.UpdateAvatarMsg(
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
