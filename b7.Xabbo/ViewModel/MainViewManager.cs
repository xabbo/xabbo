using System;
using System.Reflection;


using Wpf.Ui.Mvvm.Contracts;

using MaterialDesignThemes.Wpf;

using GalaSoft.MvvmLight;

namespace b7.Xabbo.ViewModel;

public class MainViewManager : ObservableObject
{
    private string _title = "xabbo";
    public string Title
    {
        get => _title;
        set => Set(ref _title, value);
    }

    public bool IsDebugBuild { get; }

    public IPageService PageService { get; }

    public ISnackbarMessageQueue SnackbarMessageQueue { get; }

    public GeneralViewManager General { get; }
    // Friends
    public FriendListViewManager Friends { get; }
    // Chat
    public ChatLogViewManager Chat { get; }
    // Figure
    public WardrobeViewManager Wardrobe { get; }
    public FigureRandomizerViewManager FigureRandomizer { get; }
    // Room
    public RoomInfoViewManager RoomInfo { get; }
    public EntitiesViewManager Entities { get; }
    public VisitorsViewManager Visitors { get; }
    public BanListViewManager BanList { get; }
    public FurniViewManager Furni { get; }
    // Tools
    public AlignerViewManager Aligner { get; }
    public MimicViewManager Mimic { get; }
    // Info
    public FurniDataViewManager FurniData { get; }

    public MainViewManager(
        IPageService pageService,
        ISnackbarMessageQueue snackbarMessageQueue,
        GeneralViewManager general,
        FriendListViewManager friends,
        ChatLogViewManager chat,
        WardrobeViewManager wardrobe,
        FigureRandomizerViewManager figureRandomizer,
        RoomInfoViewManager roomInfo,
        EntitiesViewManager entities,
        VisitorsViewManager visitors,
        BanListViewManager banList,
        FurniViewManager furni,
        AlignerViewManager aligner,
        MimicViewManager mimic,
        FurniDataViewManager furniData)
    {
        #if DEBUG
        IsDebugBuild = true;
        #endif

        Version? version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version is not null)
        {
            Title = $"xabbo v{version.ToString(3)}";
        }

        PageService = pageService;

        SnackbarMessageQueue = snackbarMessageQueue;

        General = general;
        Friends = friends;
        Chat = chat;
        Wardrobe = wardrobe;
        FigureRandomizer = figureRandomizer;
        RoomInfo = roomInfo;
        Entities = entities;
        Visitors = visitors;
        BanList = banList;
        Furni = furni;
        Aligner = aligner;
        Mimic = mimic;
        FurniData = furniData;
    }
}
