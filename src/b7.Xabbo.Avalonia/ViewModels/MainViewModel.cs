using System.Collections.Generic;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace b7.Xabbo.Avalonia.ViewModels;

public class MainViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Title => "xabbo";
#pragma warning restore CA1822 // Mark members as static

    public List<PageViewModel> Pages { get; set; }
    [Reactive] public PageViewModel? CurrentPage { get; set; }

    public MainViewModel() { Pages = []; }

    [DependencyInjectionConstructor]
    public MainViewModel(IControlledApplicationLifetime lt,
        GeneralPageViewModel general,
        ChatPageViewModel chat,
        RoomPageViewModel room,
        GameDataPageViewModel gameData)
    {
        Pages = [general, chat, room, gameData];
        CurrentPage = general;
    }
}
