using System;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Avalonia.Media.Imaging;

using Xabbo.Utility;
using Xabbo.Models;
using System.Reactive.Linq;

namespace Xabbo.ViewModels;

public sealed class OutfitViewModel : ViewModelBase
{
    public FigureModel Model { get; }

    public string Gender => Model.Gender;
    public string Figure => Model.FigureString;
    public bool IsOrigins => Model.IsOrigins;

    [Reactive] public string? ModernFigure { get; set; }

    private readonly ObservableAsPropertyHelper<string?> _avatarImageUrl;
     public string? AvatarImageUrl => _avatarImageUrl.Value;

    public OutfitViewModel(FigureModel model)
    {
        Model = model;

        _avatarImageUrl = this
            .WhenAnyValue(
                x => x.ModernFigure,
                (string? modernFigure) => modernFigure is null ? null : UrlHelper.AvatarImageUrl(figure: ModernFigure)
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.AvatarImageUrl);

        if (!Model.IsOrigins)
            ModernFigure = Model.FigureString;
    }
}
