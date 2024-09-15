using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web;

using Avalonia.Media.Imaging;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Xabbo.Ext.Avalonia.Helpers;

namespace Xabbo.Ext.Avalonia.ViewModels;

public sealed partial class FriendViewModel : ViewModelBase
{
    [Reactive] public Hotel Hotel { get; set; } = Hotel.None;

    public Id Id { get; set; }
    [Reactive] public bool IsOnline { get; set; }
    [Reactive] public string Name { get; set; } = "";
    [Reactive] public string Motto { get; set; } = "";
    [Reactive] public string Figure { get; set; } = "";
    [Reactive] public string? AvatarImageUrl { get; set; }
    [Reactive] public Task<Bitmap?>? AvatarImage { get; private set; }

    public FriendViewModel()
    {
        this
            .WhenAnyValue(
                x => x.Hotel,
                x => x.Name,
                x => x.Figure
            )
            .Subscribe(values => {
                var (hotel, name, figure) = values;
                AvatarImageUrl = CreateAvatarImageUrl(hotel, name, figure);
            });

        this
            .WhenAnyValue(x => x.AvatarImageUrl)
            .Subscribe(url => {
                if (string.IsNullOrWhiteSpace(url))
                    AvatarImage = null;
                else
                    AvatarImage = ImageHelper.LoadFromWeb(new Uri(url));
            });
    }

    private static string? CreateAvatarImageUrl(Hotel hotel, string name, string figure)
    {
        if (hotel == Hotel.None)
            return null;

        var query = HttpUtility.ParseQueryString("");
        query.Add("headonly", "1");
        query.Add("direction", "2");
        if (string.IsNullOrWhiteSpace(figure))
            query.Add("user", name);
        else
            query.Add("figure", figure);

        return $"https://{hotel.WebHost}/habbo-imaging/avatarimage?{query}";
    }
}