using System;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Avalonia.Media.Imaging;

using Xabbo.Utility;
using Xabbo.Models;

namespace Xabbo.ViewModels;

public sealed class OutfitViewModel : ViewModelBase
{
    public FigureModel Model { get; }

    public string Gender => Model.Gender;
    public string Figure => Model.FigureString;
    public bool IsOrigins => Model.IsOrigins;

    [Reactive] public string? ModernFigure { get; set; }
    [Reactive] public string? AvatarImageUrl { get; set; }
    [Reactive] public Task<Bitmap?>? AvatarImage { get; private set; }

    public OutfitViewModel(FigureModel model)
    {
        Model = model;

        this
            .WhenAnyValue(x => x.ModernFigure)
            .Subscribe(values => {
                AvatarImageUrl = ModernFigure is null ? null : UrlHelper.AvatarImageUrl(figure: ModernFigure);
            });

        this
            .WhenAnyValue(x => x.AvatarImageUrl)
            .Subscribe(url => {
                AvatarImage = url is not null ? ImageHelper.LoadFromWeb(new Uri(url)) : null;
            });

        if (!Model.IsOrigins)
            ModernFigure = Model.FigureString;
    }
}
