using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Diagnostics;

using Microsoft.Extensions.Hosting;

using GalaSoft.MvvmLight.Command;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;

using b7.Xabbo.Services;
using b7.Xabbo.Model;

namespace b7.Xabbo.ViewModel;

public class WardrobeViewManager : ComponentViewModel
{
    private static readonly Regex regexFigure = new Regex(@"[a-z]{2}(-\d+){1,3}(\.[a-z]{2}(-\d+){1,3})*");

    private readonly IUiContext _uiContext;
    private readonly IWardrobeRepository _repository;
    private readonly ProfileManager _profileManager;

    private readonly ObservableCollection<FigureViewModel> _figureViewModels;
    public ICollectionView Figures { get; private set; }

    public ICommand AddFromClipboardCommand { get; }
    public ICommand AddCurrentFigureCommand { get; }
    public ICommand SetFigureCommand { get; }
    public ICommand RemoveFigureCommand { get; }
    public ICommand RemoveFiguresCommand { get; }
    public ICommand CopyFigureCommand { get; }
    public ICommand ImportWardrobeCommand { get; }

    private bool _isAvailable;
    public bool IsAvailable
    {
        get => _isAvailable;
        set => Set(ref _isAvailable, value);
    }

    public WardrobeViewManager(IHostApplicationLifetime _lifetime,
        IExtension extension,
        IUiContext context,
        IWardrobeRepository repository,
        ProfileManager profileManager)
        : base(extension)
    {
        _uiContext = context;
        _repository = repository;
        _profileManager = profileManager;

        _figureViewModels = new ObservableCollection<FigureViewModel>();
        Figures = CollectionViewSource.GetDefaultView(_figureViewModels);

        AddFromClipboardCommand = new RelayCommand(OnAddFromClipboard);
        AddCurrentFigureCommand = new RelayCommand(OnAddCurrentFigure);
        SetFigureCommand = new RelayCommand<FigureViewModel>(OnSetFigure);
        RemoveFigureCommand = new RelayCommand<FigureViewModel>(OnRemoveFigure);
        RemoveFiguresCommand = new RelayCommand<IList>(OnRemoveFigures);
        CopyFigureCommand = new RelayCommand<FigureViewModel>(OnCopyFigure);
        ImportWardrobeCommand = new RelayCommand(OnImportWardrobe);

        _lifetime.ApplicationStarted.Register(() => Task.Run(InitializeAsync));
    }

    private async Task InitializeAsync()
    {
        try
        {
            _repository.Initialize();
        }
        catch (Exception ex)
        {
            _uiContext.Invoke(() => Dialog.ShowError($"Failed to initialize wardrobe database: {ex.Message}"));
            return;
        }

        var knownFigures = new HashSet<string>();
        var figureModels = _repository.Load().ToList();

        foreach (var figureModel in figureModels.OrderBy(x => x.Order))
        {
            if (!Figure.TryParse(figureModel.FigureString, out Figure? figure) ||
                !knownFigures.Add(figure.GetFigureString()))
            {
                continue;
            }

            try { figure.Gender = H.ToGender(figureModel.Gender); }
            catch { }

            if (figure.Gender != Gender.Male &&
                figure.Gender != Gender.Female)
            {
                figure.Gender = Gender.Male;

                figureModel.Gender = "M";
                _repository.Update(figureModel);
            }

            await _uiContext.InvokeAsync(() => _figureViewModels.Add(new FigureViewModel(figureModel, figure)));
        }

        UpdateSortIndices();

        try { await _profileManager.GetUserDataAsync(); }
        catch { return; }

        IsAvailable = true;
    }

    private void UpdateSortIndices()
    {
        List<FigureModel> updated = new List<FigureModel>();
        for (int i = 0; i < _figureViewModels.Count; i++)
        {
            if (_figureViewModels[i].Order != i)
            {
                _figureViewModels[i].Order = i;
                updated.Add(_figureViewModels[i].Model);
            }
        }

        _repository.Update(updated);
    }

    private void AddFigure(Figure figure)
    {
        if (!_uiContext.IsSynchronized)
        {
            _uiContext.InvokeAsync(() => AddFigure(figure));
            return;
        }

        var figureModel = new FigureModel()
        {
            FigureString = figure.GetFigureString(),
            Order = _figureViewModels.Count
        };

        if (!_repository.Insert(figureModel))
            return;

        _figureViewModels.Add(new FigureViewModel(figureModel, figure));
    }

    private void RemoveFigure(FigureViewModel figureViewModel)
    {
        if (!_uiContext.IsSynchronized)
        {
            _uiContext.InvokeAsync(() => RemoveFigure(figureViewModel));
            return;
        }

        if (!_repository.Delete(figureViewModel.Model))
            return;

        _figureViewModels.Remove(figureViewModel);
        UpdateSortIndices();
    }

    private void RemoveFigures(IEnumerable<FigureViewModel> figures)
    {
        if (!_uiContext.IsSynchronized)
        {
            _uiContext.InvokeAsync(() => RemoveFigures(figures));
            return;
        }

        _repository.Delete(figures.Select(x => x.Model));

        foreach (var figure in figures)
            _figureViewModels.Remove(figure);

        UpdateSortIndices();
    }

    #region - Commands -
    private void OnAddFromClipboard()
    {
        if (!Clipboard.ContainsText()) return;

        try
        {
            string text = Clipboard.GetText();
            foreach (Match match in regexFigure.Matches(text))
            {
                if (Figure.TryParse(match.Value, out Figure? figure))
                    AddFigure(figure);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WardrobeView] Error adding figure: {ex.Message}");
        }
    }

    private void OnAddCurrentFigure()
    {
        if (!IsAvailable || _profileManager.UserData is null) return;

        if (!Figure.TryParse(_profileManager.UserData.Figure, out Figure? figure))
            return;

        AddFigure(figure);
    }

    private async void OnSetFigure(FigureViewModel figureViewModel)
    {
        if (!IsAvailable) return;

        await Extension.SendAsync(Out.UpdateAvatar,
            figureViewModel.Figure.GetGenderString(),
            figureViewModel.Figure.GetFigureString()
        );
    }

    private void OnRemoveFigure(FigureViewModel figureViewModel)
    {
        RemoveFigure(figureViewModel);
    }

    private void OnRemoveFigures(IList selected)
    {
        if (!IsAvailable || selected == null) return;

        RemoveFigures(selected.OfType<FigureViewModel>().ToArray());
    }

    private void OnCopyFigure(FigureViewModel figureViewModel)
    {
        try { Clipboard.SetText(figureViewModel.Figure.GetFigureString()); }
        catch { }
    }

    private async void OnImportWardrobe()
    {
        try
        {
            await Extension.SendAsync(Out.GetWardrobe);
            var packet = await Extension.ReceiveAsync(In.UserWardrobe, 5000);
            int state = packet.ReadInt();
            short n = packet.ReadLegacyShort();
            for (int i = 0; i < n; i++)
            {
                int slot = packet.ReadInt();
                string figureString = packet.ReadString();
                Gender gender = H.ToGender(packet.ReadString());
                if (Figure.TryParse(figureString, out Figure? figure))
                {
                    figure.Gender = gender;
                    AddFigure(figure);
                }
            }
        }
        catch { }
    }
    #endregion

    public void MoveFigure(FigureViewModel from, FigureViewModel to)
    {
        int insertionIndex = _figureViewModels.IndexOf(to);
        _figureViewModels.Remove(from);
        _figureViewModels.Insert(insertionIndex, from);

        for (int i = 0; i < _figureViewModels.Count; i++)
        {
            if (_figureViewModels[i].Order != i)
            {
                _figureViewModels[i].Order = i;
                _repository.Update(_figureViewModels[i].Model);
            }
        }
    }
}
