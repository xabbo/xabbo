using Xabbo.Models;

namespace Xabbo.ViewModels;

public sealed class OutfitViewModel : ViewModelBase
{
    public FigureModel Model { get; }

    public string Gender => Model.Gender;
    public string Figure => Model.FigureString;
    public bool IsOrigins => Model.IsOrigins;

    [Reactive] public string? ModernFigure { get; set; }

    public OutfitViewModel(FigureModel model)
    {
        Model = model;

        if (!Model.IsOrigins)
            ModernFigure = Model.FigureString;
    }
}
