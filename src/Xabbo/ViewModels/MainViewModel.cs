using System.Reactive;
using ReactiveUI;
using Splat;
using Xabbo.Configuration;
using Xabbo.Services.Abstractions;

namespace Xabbo.ViewModels;

public class MainViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Title => "xabbo";
#pragma warning restore CA1822 // Mark members as static

    private readonly ILauncherService _launcher;

    public List<PageViewModel> Pages { get; set; }
    public List<PageViewModel> FooterPages { get; set; }
    [Reactive] public PageViewModel? SelectedPage { get; set; }

    private readonly IConfigProvider<AppConfig>? _config;
    public AppConfig? Config => _config?.Value;

    [Reactive] public string? AppError { get; set; }

    public ReactiveCommand<Unit, Unit> ReportErrorCmd { get; }

    public MainViewModel()
    {
        Pages = [];
        FooterPages = [];
        ReportErrorCmd = null!;
        _launcher = null!;
    }

    [DependencyInjectionConstructor]
    public MainViewModel(
        IConfigProvider<AppConfig> config,
        GeneralPageViewModel general,
        WardrobePageViewModel wardrobe,
        InventoryPageViewModel inventory,
        FriendsPageViewModel friends,
        ChatPageViewModel chat,
        RoomPageViewModel room,
        GameDataPageViewModel gameData,
        InfoPageViewModel info,
        SettingsPageViewModel settings,
        ILauncherService launcher)
    {
        _config = config;
        Pages = [general, wardrobe, inventory, friends, chat, room, gameData];
        FooterPages = [info, settings];
        SelectedPage = general;

        _launcher = launcher;

        ReportErrorCmd = ReactiveCommand.Create(ReportError);
    }

    private void ReportError()
    {
        _launcher.Launch("https://github.com/xabbo/xabbo/issues/new", new() {
            ["body"] = [$"(describe the issue here)\n\n```txt\n{AppError}\n```"]
        });
    }
}
