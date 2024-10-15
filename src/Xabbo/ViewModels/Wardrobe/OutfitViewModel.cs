using System.Reactive;
using ReactiveUI;
using Xabbo.Models;

namespace Xabbo.ViewModels;

public sealed class OutfitViewModel : ViewModelBase
{
    public FigureModel Model { get; }

    public string Gender => Model.Gender;
    public string Figure => Model.FigureString;
    public bool IsOrigins => Model.IsOrigins;

    [Reactive] public int Direction { get; set; } = 2;
    [Reactive] public string? ModernFigure { get; set; }

    public ReactiveCommand<int, Unit> RotateCmd { get; }

    public OutfitViewModel(FigureModel model)
    {
        Model = model;

        if (!Model.IsOrigins)
            ModernFigure = Model.FigureString;

        RotateCmd = ReactiveCommand.Create<int>(Rotate);
    }

    private void Rotate(int amount)
    {
        using (DelayChangeNotifications())
        {
            Direction = (Direction + amount) % 8;
            if (Direction < 0) Direction += 8;
        }
    }
}
