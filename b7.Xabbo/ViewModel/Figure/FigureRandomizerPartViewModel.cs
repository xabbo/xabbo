using System;
using System.Collections.Generic;
using System.Windows.Media;

using GalaSoft.MvvmLight;

using Xabbo.Core;

namespace b7.Xabbo.ViewModel
{
    public class FigureRandomizerPartViewModel : ObservableObject
    {
        private FigurePartType _type;
        public FigurePartType Type
        {
            get => _type;
            set => Set(ref _type, value);
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _shortString = string.Empty;
        public string ShortString
        {
            get => _shortString;
            set => Set(ref _shortString, value);
        }

        private double _probability;
        public double Probability
        {
            get => _probability;
            set => Set(ref _probability, value);
        }

        private bool _lockPart;
        public bool IsLocked
        {
            get => _lockPart;
            set => Set(ref _lockPart, value);
        }

        public IReadOnlyList<FigurePartColorViewModel> Colors { get; }

        public FigureRandomizerPartViewModel(FigurePartType type, string name, double probability)
        {
            Type = type;
            Name = name;
            ShortString = type.ToShortString();
            Probability = probability;

            // 2 colors max ...
            Colors = new List<FigurePartColorViewModel>()
            {
                new FigurePartColorViewModel(),
                new FigurePartColorViewModel()
            };
        }
    }

    public class FigurePartColorViewModel : ViewModelBase
    {
        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => Set(ref _isVisible, value);
        }

        private bool _isLocked;
        public bool IsLocked
        {
            get => _isLocked;
            set => Set(ref _isLocked, value);
        }

        private SolidColorBrush _foreground = Brushes.Transparent;
        public SolidColorBrush Foreground
        {
            get => _foreground;
            set => Set(ref _foreground, value);
        }

        private SolidColorBrush _background = Brushes.Transparent;
        public SolidColorBrush Background
        {
            get => _background;
            set => Set(ref _background, value);
        }
    }
}
