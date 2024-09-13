using System;
using System.Collections.Generic;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
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
    [Reactive] public PageViewModel? CurrentPage { get; set; }

    [Reactive] public bool IsConnected { get; set; }

    public MainViewModel() { Pages = []; }

    [DependencyInjectionConstructor]
    public MainViewModel(IControlledApplicationLifetime lt,
        GEarthExtension extension,
        GeneralPageViewModel general,
        ChatPageViewModel chat,
        RoomPageViewModel room,
        GameDataPageViewModel gameData)
    {
        _extension = extension;
        _extension.Connected += OnGameConnected;

        Pages = [general, chat, room, gameData];
        CurrentPage = general;
    }

    private void OnGameConnected(GameConnectedArgs args)
    {
        IsConnected = true;
    }
}
