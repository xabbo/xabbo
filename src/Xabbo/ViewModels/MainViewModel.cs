using Splat;
using Xabbo.Configuration;
using Xabbo.Services.Abstractions;

namespace Xabbo.ViewModels;

public class MainViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Title => "xabbo";
#pragma warning restore CA1822 // Mark members as static

    public List<PageViewModel> Pages { get; set; }
    public List<PageViewModel> FooterPages { get; set; }
    [Reactive] public PageViewModel? SelectedPage { get; set; }

    private readonly IConfigProvider<AppConfig>? _config;
    public AppConfig? Config => _config?.Value;

    public MainViewModel()
    {
        Pages = [];
        FooterPages = [];
    }

    [DependencyInjectionConstructor]
    public MainViewModel(
        IConfigProvider<AppConfig> config,
        GeneralPageViewModel general,
        WardrobePageViewModel wardrobe,
        FriendsPageViewModel friends,
        ChatPageViewModel chat,
        RoomPageViewModel room,
        GameDataPageViewModel gameData,
        InfoPageViewModel info,
        SettingsPageViewModel settings)
    {
        _config = config;
        Pages = [general, wardrobe, friends, chat, room, gameData];
        FooterPages = [info, settings];
        SelectedPage = general;
    }
}
