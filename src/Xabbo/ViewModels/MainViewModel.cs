using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;
using Splat;
using Avalonia.Controls.ApplicationLifetimes;

using Xabbo.Extension;

namespace Xabbo.ViewModels;

public class MainViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Title => "xabbo";
#pragma warning restore CA1822 // Mark members as static

    public List<PageViewModel> Pages { get; set; }
    public List<PageViewModel> FooterPages { get; set; }
    [Reactive] public PageViewModel? SelectedPage { get; set; }

    public MainViewModel()
    {
        Pages = [];
        FooterPages = [];
    }

    [DependencyInjectionConstructor]
    public MainViewModel(
        GeneralPageViewModel general,
        WardrobePageViewModel wardrobe,
        FriendsPageViewModel friends,
        ChatPageViewModel chat,
        RoomPageViewModel room,
        SettingsPageViewModel settings,
        GameDataPageViewModel gameData)
    {
        Pages = [general, wardrobe, friends, chat, room, gameData];
        FooterPages = [settings];
        SelectedPage = general;
    }
}
