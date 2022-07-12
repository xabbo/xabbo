using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.Extensions.Hosting;

using GalaSoft.MvvmLight.Command;

using Xabbo.Interceptor;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;

namespace b7.Xabbo.ViewModel
{
    public class FigureRandomizerViewManager : ComponentViewModel
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IGameDataManager _gameData;
        private readonly ProfileManager _profileManager;

        private FigureData? FigureData => _gameData.Figure;

        private FigureRandomizer? _figureRandomizer;
        private Figure? _baseFigure;

        private CancellationTokenSource? _timerCts;

        private readonly Dictionary<FigurePartType, FigureRandomizerPartViewModel> figureRandomizerParts;

        public ObservableCollection<FigureRandomizerPartViewModel> FigurePartOptions { get; }

        public IReadOnlyList<Gender> Genders { get; } = new [] { Gender.Male, Gender.Female };

        private bool _isReady;
        public bool IsReady
        {
            get => _isReady;
            private set => Set(ref _isReady, value);
        }

        private int _timerInterval = 5500;
        public int TimerInterval
        {
            get => _timerInterval;
            set => Set(ref _timerInterval, value);
        }

        private bool _isTimerActive;
        public bool IsTimerActive
        {
            get => _isTimerActive;
            private set => Set(ref _isTimerActive, value);
        }

        private bool _useBaseFigure;
        public bool UseBaseFigure
        {
            get => _useBaseFigure;
            set => Set(ref _useBaseFigure, value);
        }

        private string _baseFigureString = string.Empty;
        public string BaseFigureString
        {
            get => _baseFigureString;
            set
            {
                if (Set(ref _baseFigureString, value))
                    UpdateBaseFigure();
            }
        }

        private Gender _baseFigureGender = Gender.Male;
        public Gender BaseFigureGender
        {
            get => _baseFigureGender;
            set => Set(ref _baseFigureGender, value);
        }

        private bool _allowHC;
        public bool AllowHC
        {
            get => _allowHC;
            set => Set(ref _allowHC, value);
        }

        public ICommand GetCurrentFigureCommand { get; }
        public ICommand RandomizeLooksCommand { get; }
        public ICommand StartStopTimerCommand { get; }

        public FigureRandomizerViewManager(
            IHostApplicationLifetime lifetime,
            IInterceptor interceptor,
            IGameDataManager gameDataManager,
            ProfileManager profileManager)
            : base(interceptor)
        {
            _lifetime = lifetime;
            _gameData = gameDataManager;
            _profileManager = profileManager;

            FigurePartOptions = new ObservableCollection<FigureRandomizerPartViewModel>()
            {
                new FigureRandomizerPartViewModel(FigurePartType.Hair, "Hair", 100.0),
                new FigureRandomizerPartViewModel(FigurePartType.Head, "Face (skin)", 100.0),
                new FigureRandomizerPartViewModel(FigurePartType.Chest, "Chest", 100.0),
                new FigureRandomizerPartViewModel(FigurePartType.Legs, "Legs", 100.0),
                new FigureRandomizerPartViewModel(FigurePartType.Shoes, "Shoes", 70.0),
                new FigureRandomizerPartViewModel(FigurePartType.Hat, "Hat", 30.0),
                new FigureRandomizerPartViewModel(FigurePartType.HeadAccessory, "Head accessory", 10.0),
                new FigureRandomizerPartViewModel(FigurePartType.EyeAccessory, "Eye accessory", 10.0),
                new FigureRandomizerPartViewModel(FigurePartType.FaceAccessory, "Face accessory", 10.0),
                new FigureRandomizerPartViewModel(FigurePartType.ChestAccessory, "Chest accessory", 10.0),
                new FigureRandomizerPartViewModel(FigurePartType.WaistAccessory, "Waist accessory", 10.0),
                new FigureRandomizerPartViewModel(FigurePartType.Coat, "Coat", 10.0),
                new FigureRandomizerPartViewModel(FigurePartType.ChestPrint, "Shirt decoration", 10.0)
            };

            figureRandomizerParts = FigurePartOptions.ToDictionary(x => x.Type);

            GetCurrentFigureCommand = new RelayCommand(OnGetCurrentFigure);
            RandomizeLooksCommand = new RelayCommand(OnRandomizeLooks);
            StartStopTimerCommand = new RelayCommand(OnStartStopTimer);

            Interceptor.Connected += OnGameConnected;
            Interceptor.Disconnected += OnGameDisconnected;
        }

        private async void OnGameConnected(object? sender, GameConnectedEventArgs e)
        {
            await _gameData.WaitForLoadAsync(CancellationToken.None);
            _figureRandomizer = new FigureRandomizer(_gameData.Figure!);

            await _profileManager.GetUserDataAsync();

            IsReady = true;
        }

        private void OnGameDisconnected(object? sender, EventArgs e)
        {
            IsReady = false;
        }

        private bool SetupRandomizer()
        {
            if (_figureRandomizer is null) return false;

            try
            {
                foreach (var part in figureRandomizerParts.Values)
                    _figureRandomizer.Probabilities[part.Type] = part.Probability / 100.0;

                _figureRandomizer.AllowHC = AllowHC;

                if (UseBaseFigure)
                    _baseFigure = Figure.Parse(BaseFigureString);
            }
            catch { return false; }

            return true;
        }

        private async void OnStartStopTimer()
        {
            if (IsTimerActive)
            {
                _timerCts?.Cancel();
                return;
            }

            if (!SetupRandomizer())
                return;

            if (TimerInterval < 100)
                TimerInterval = 100;

            try
            {
                _timerCts = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.ApplicationStopping);
                var token = _timerCts.Token;

                IsTimerActive = true;

                while (!token.IsCancellationRequested)
                {
                    await SendRandomFigure();
                    await Task.Delay(TimerInterval, token);
                }
            }
            catch { }
            finally
            {
                _timerCts?.Dispose();
                _timerCts = null;

                IsTimerActive = false;
            }
        }

        private async void OnRandomizeLooks()
        {
            if (!SetupRandomizer())
                return;

            await SendRandomFigure();
        }

        private void OnGetCurrentFigure()
        {
            if (_profileManager.UserData != null)
            {
                BaseFigureString = _profileManager.UserData.Figure;
                BaseFigureGender = _profileManager.UserData.Gender;
            }
        }

        private void UpdateBaseFigure()
        {
            if (!IsReady || FigureData is null) return;

            try
            {
                foreach (var color in FigurePartOptions.SelectMany(x => x.Colors))
                {
                    color.IsVisible = false;
                    color.Background = Brushes.Transparent;
                }

                var figure = Figure.Parse(BaseFigureString);
                foreach (var part in figure.Parts)
                {
                    if (part.Colors.Count == 0)
                        continue;

                    var figurePartViewModel = figureRandomizerParts[part.Type];

                    for (int i = 0; i < part.Colors.Count; i++)
                    {
                        var set = FigureData.GetSetCollection(part.Type);
                        if (set == null) continue;
                        var palette = FigureData.GetPalette(set.PaletteId);
                        if (palette == null) continue;
                        var color = palette.GetColor(part.Colors[i]);
                        if (color == null) continue;

                        byte r = byte.Parse(color.Value[0..2], NumberStyles.HexNumber);
                        byte g = byte.Parse(color.Value[2..4], NumberStyles.HexNumber);
                        byte b = byte.Parse(color.Value[4..6], NumberStyles.HexNumber);

                        var colorViewModel = figurePartViewModel.Colors[i];

                        colorViewModel.IsVisible = true;
                        colorViewModel.Background = new SolidColorBrush(
                            new Color()
                            {
                                R = r,
                                G = g,
                                B = b,
                                A = 255
                            }
                        );

                        colorViewModel.Foreground = new SolidColorBrush(
                            new Color()
                            {
                                R = (byte)(255 - r),
                                G = (byte)(255 - g),
                                B = (byte)(255 - b),
                                A = 255
                            }
                        );
                    }
                }

                if (FigureData.TryGetGender(figure, out Gender gender))
                    BaseFigureGender = gender;
            }
            catch (Exception ex) { Debug.WriteLine($"[FigureRandomizerView] Error: {ex.Message}"); }
        }

        private async Task SendRandomFigure()
        {
            if (_figureRandomizer is null ||
                _baseFigure is null) return;

            Figure figure;
            if (_useBaseFigure)
            {
                var checkedParts = new List<FigurePartType>();
                var partsToRemove = new List<FigurePartType>();
                figure = _figureRandomizer.Generate(BaseFigureGender);

                // Iterate through each part in the randomized figure
                foreach (var part in figure.Parts)
                {
                    var basePart = _baseFigure[part.Type];

                    var partViewModel = figureRandomizerParts[part.Type];

                    // If this part is locked
                    if (partViewModel.IsLocked)
                    {
                        // And the base figure has this part
                        if (basePart != null)
                        {
                            // Set the part on the randomized figure
                            part.Id = basePart.Id;
                            // Re-randomize colors as the randomly generated part
                            // could have a different number of colors to the base part
                            _figureRandomizer.RandomizeColors(part);
                        }
                        else
                            // Otherwise save this part type for removal
                            partsToRemove.Add(part.Type);
                    }

                    // If the base has this part
                    if (basePart != null)
                    {
                        // Iterate through each color 
                        for (int i = 0; i < basePart.Colors.Count && i < part.Colors.Count; i++)
                        {
                            // If the color at this index for this part type is locked
                            if (partViewModel.Colors[i].IsLocked)
                            {
                                // Set the color on the randomized figure
                                part.Colors[i] = basePart.Colors[i];
                            }
                        }
                    }

                    checkedParts.Add(part.Type);
                }

                // Remove each part that is locked and not set in the base figure
                foreach (var type in partsToRemove)
                    figure.RemovePart(type);

                // Iterate through each part in the base figure that doesn't exist in the randomized figure
                foreach (var part in _baseFigure.Parts)
                {
                    if (checkedParts.Contains(part.Type))
                        continue;

                    var partViewModel = figureRandomizerParts[part.Type];

                    if (partViewModel.IsLocked)
                    {
                        for (int i = 0; i < part.Colors.Count; i++)
                        {
                            if (!partViewModel.Colors[i].IsLocked)
                                part.Colors[i] = _figureRandomizer.GetRandomColor(part.Type).Id;
                        }
                        figure.AddPart(part);
                    }
                    checkedParts.Add(part.Type);
                }
            }
            else
                figure = _figureRandomizer.Generate();

            await Interceptor.SendAsync(Out.UpdateAvatar, figure.GetGenderString(), figure.GetFigureString());
        }
    }
}
