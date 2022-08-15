using System;

using Xabbo.Core;

using b7.Xabbo.Model;
using GalaSoft.MvvmLight;

namespace b7.Xabbo.ViewModel;

public class FigureViewModel : ObservableObject
{
    public FigureModel Model { get; }
    public Figure Figure { get; }

    public string ImageUrl { get; }

    public int Order
    {
        get => Model.Order;
        set
        {
            if (Model.Order == value) return;
            Model.Order = value;
            RaisePropertyChanged(nameof(Order));
        }
    }

    public FigureViewModel(FigureModel entry, Figure figure)
    {
        Model = entry;
        Figure = figure;

        ImageUrl =
            $"https://www.habbo.com/habbo-imaging/avatarimage" +
            $"?size=m" +
            $"&figure={figure.GetFigureString()}" +
            $"&direction=4" +
            $"&head_direction=4";
    }
}
