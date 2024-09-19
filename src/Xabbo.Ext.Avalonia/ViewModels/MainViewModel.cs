using System.Collections.Generic;

using Avalonia.Controls.ApplicationLifetimes;

using ReactiveUI.Fody.Helpers;

using Splat;

using Xabbo.GEarth;

namespace Xabbo.Ext.Avalonia.ViewModels;

public class MainViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Title => "xabbo";
#pragma warning restore CA1822 // Mark members as static

    private readonly GEarthExtension _extension;

    public List<PageViewModel> Pages { get; set; }
    public List<PageViewModel> FooterPages { get; set; }
    [Reactive] public PageViewModel? SelectedPage { get; set; }

    [Reactive] public bool IsConnected { get; set; }

    public MainViewModel()
    {
        _extension = null!;
        Pages = [];
        FooterPages = [];
    }

    [DependencyInjectionConstructor]
    public MainViewModel(IControlledApplicationLifetime lt,
        GEarthExtension extension,
        GeneralPageViewModel general,
        WardrobePageViewModel wardrobe,
        FriendsPageViewModel friends,
        ChatPageViewModel chat,
        RoomPageViewModel room,
        SettingsPageViewModel settings,
        GameDataPageViewModel gameData)
    {
        _extension = extension;
        _extension.Connected += OnGameConnected;

        Pages = [general, wardrobe, friends, chat, room, gameData];
        FooterPages = [settings];
        SelectedPage = general;
    }

    private void OnGameConnected(GameConnectedArgs args)
    {
        IsConnected = true;
    }
}
