using System.Reactive.Linq;

using ReactiveUI;

using Xabbo.Utility;

namespace Xabbo.ViewModels;

public sealed partial class FriendViewModel : ViewModelBase
{
    public Id Id { get; set; }
    [Reactive] public bool IsOnline { get; set; }
    [Reactive] public string Name { get; set; } = "";
    [Reactive] public string Motto { get; set; } = "";
    [Reactive] public string Figure { get; set; } = "";
    [Reactive] public bool IsModernFigure { get; set; }
    [Reactive] public string? AvatarImageUrl { get; set; }

    public FriendViewModel()
    {
        this
            .WhenAnyValue(
                x => x.Name,
                x => x.Figure,
                x => x.IsModernFigure
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(values => {
                var (name, figure, isModern) = values;
                AvatarImageUrl = isModern ? UrlHelper.AvatarImageUrl(name, figure, headOnly: true) : null;
            });
    }
}
