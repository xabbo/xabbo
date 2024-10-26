using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Splat;
using Avalonia.Logging;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;

using Xabbo.Interceptor;
using Xabbo.Extension;
using Xabbo.GEarth;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Configuration;
using Xabbo.Services.Abstractions;
using Xabbo.Services;
using Xabbo.Avalonia.Services;
using Xabbo.Components;
using Xabbo.Controllers;
using Xabbo.Command;
using Xabbo.Command.Modules;
using Xabbo.ViewModels;
using Xabbo.Avalonia.Views;

using IHostApplicationLifetime = Microsoft.Extensions.Hosting.IHostApplicationLifetime;
using Splatr = Splat.SplatRegistrations;

namespace Xabbo.Avalonia;

public static class ViewModelLocator
{
    static ViewModelLocator()
    {
        RegisterConfiguration();
        RegisterLogging();
        RegisterServices();
        RegisterRepositories();
        RegisterComponents();
        RegisterControllers();
        RegisterCommands();
        RegisterViewModels();
        RegisterViews();

        Splatr.SetupIOC();
    }

    static void RegisterConfiguration()
    {
        var configRoot = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
#if DEBUG
            .AddJsonFile("appsettings.development.json", optional: true)
#endif
            .Build();
        Splatr.RegisterConstant<IConfiguration>(configRoot);
    }

    static void RegisterLogging()
    {

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => {
            builder
                .AddConfiguration(Locator.Current.GetRequiredService<IConfiguration>().GetSection("Logging"))
#if RELEASE
                .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.None)
#endif
                .AddSimpleConsole(console =>
                {
                    console.SingleLine = true;
                    console.IncludeScopes = true;
                    console.TimestampFormat = "[HH:mm:ss.fff] ";
                });
        });
        Locator.CurrentMutable.RegisterConstant(loggerFactory);

        Logger.Sink = new AvaloniaLogSink(loggerFactory);
    }

    static void RegisterServices()
    {
        Splatr.RegisterConstant(new GEarthOptions() with
        {
            Name = "xabbo",
            Description = "enhanced habbo",
            Author = "b7"
        });
        Splatr.RegisterLazySingleton<GEarthExtension>();

        Locator.CurrentMutable.Register<IExtension>(() => Locator.Current.GetService<GEarthExtension>());
        Locator.CurrentMutable.Register<IRemoteExtension>(() => Locator.Current.GetService<GEarthExtension>());
        Locator.CurrentMutable.Register<IInterceptor>(() => Locator.Current.GetService<GEarthExtension>());

        Splatr.RegisterLazySingleton<IHostApplicationLifetime, AvaloniaHostApplicationLifetime>();
        Splatr.RegisterLazySingleton<IApplicationManager, XabboAppManager>();
        Splatr.RegisterLazySingleton<IUiContext, AvaloniaUiContext>();
        Splatr.RegisterLazySingleton<IClipboardService, ClipboardService>();
        Splatr.RegisterLazySingleton<ILauncherService, LauncherService>();
        Splatr.RegisterLazySingleton<GlobalExceptionHandler>();

        // Configuration
        Splatr.RegisterLazySingleton<IAppPathProvider, AppPathService>();
        Splatr.RegisterLazySingleton<IConfigProvider<AppConfig>, AppConfigProvider>();

        Locator.CurrentMutable.RegisterLazySingleton(() => (IDialogService)new DialogService(
            new DialogManager(
                viewLocator: new ViewLocator(),
                dialogFactory: new DialogFactory().AddFluent(FluentMessageBoxType.ContentDialog)),
            viewModelFactory: x => Locator.Current.GetService(x)));

        // Xabbo services
        Splatr.RegisterLazySingleton<IHabboApi, HabboApi>();
        Splatr.RegisterLazySingleton<IFigureConverterService, FigureConverterService>();
        Splatr.RegisterLazySingleton<IGameDataManager, GameDataManager>();
        Splatr.RegisterLazySingleton<IGameStateService, GameStateService>();
        Splatr.RegisterLazySingleton<IOperationManager, OperationManager>();
        Splatr.RegisterLazySingleton<IPlacementFactory, PlacementFactory>();

        // Xabbo core components
        Splatr.RegisterLazySingleton<ProfileManager>();
        Splatr.RegisterLazySingleton<InventoryManager>();
        Splatr.RegisterLazySingleton<RoomManager>();
        Splatr.RegisterLazySingleton<TradeManager>();
        Splatr.RegisterLazySingleton<FriendManager>();
    }

    static void RegisterRepositories()
    {
        Splatr.RegisterLazySingleton<IWardrobeRepository, JsonWardrobeRepository>();
    }

    static void RegisterComponents()
    {
        Splatr.RegisterLazySingleton<XabbotComponent>();
        Splatr.RegisterLazySingleton<NotificationComponent>();
        Splatr.RegisterLazySingleton<AvatarOverlayComponent>();

        Splatr.RegisterLazySingleton<AntiHandItemComponent>();
        Splatr.RegisterLazySingleton<AntiHcGiftNotificationComponent>();
        Splatr.RegisterLazySingleton<AntiIdleComponent>();
        Splatr.RegisterLazySingleton<AntiTradeComponent>();
        Splatr.RegisterLazySingleton<AntiTurnComponent>();
        Splatr.RegisterLazySingleton<AntiTypingComponent>();
        Splatr.RegisterLazySingleton<AntiWalkComponent>();
        Splatr.RegisterLazySingleton<ClickThroughComponent>();
        Splatr.RegisterLazySingleton<RespectedComponent>();

        Splatr.RegisterLazySingleton<ChatComponent>();
        Splatr.RegisterLazySingleton<DoorbellComponent>();
        Splatr.RegisterLazySingleton<FlattenRoomComponent>();
        Splatr.RegisterLazySingleton<FurniActionsComponent>();
        Splatr.RegisterLazySingleton<RoomEntryComponent>();
        Splatr.RegisterLazySingleton<LightingComponent>();
    }

    static void RegisterControllers()
    {
        Splatr.RegisterLazySingleton<ControllerInitializer>();

        Splatr.RegisterLazySingleton<ClickToController>();
        Splatr.RegisterLazySingleton<RoomAvatarsController>();
        Splatr.RegisterLazySingleton<RoomRightsController>();
        Splatr.RegisterLazySingleton<RoomModerationController>();
        Splatr.RegisterLazySingleton<RoomFurniController>();
        Splatr.RegisterLazySingleton<FurniPlacementController>();
        Splatr.RegisterLazySingleton<PrivacyController>();
    }

    static void RegisterCommands()
    {
        Locator.CurrentMutable.RegisterLazySingleton(() => Locator.Current.GetServices<CommandModule>());

        Splatr.RegisterLazySingleton<CommandManager>();

        #pragma warning disable SPLATDI006
        Splatr.RegisterLazySingleton<CommandModule, AppCommands>();
        Splatr.RegisterLazySingleton<CommandModule, OperationCommands>();
        Splatr.RegisterLazySingleton<CommandModule, EffectCommands>();
        Splatr.RegisterLazySingleton<CommandModule, FindFriendCommand>();
        Splatr.RegisterLazySingleton<CommandModule, VisibilityCommands>();
        Splatr.RegisterLazySingleton<CommandModule, FurniCommands>();
        Splatr.RegisterLazySingleton<CommandModule, InfoCommands>();
        Splatr.RegisterLazySingleton<CommandModule, ModerationCommands>();
        Splatr.RegisterLazySingleton<CommandModule, MoodCommands>();
        Splatr.RegisterLazySingleton<CommandModule, RoomCommands>();
        Splatr.RegisterLazySingleton<CommandModule, TurnCommand>();
        Splatr.RegisterLazySingleton<CommandModule, UserProfileCommands>();
        Splatr.RegisterLazySingleton<CommandModule, ClickThroughCommand>();
        #pragma warning restore SPLATDI006
    }

    static void RegisterViewModels()
    {
        Splatr.RegisterLazySingleton<MainViewModel>();

        Splatr.RegisterLazySingleton<GeneralPageViewModel>();
        Splatr.RegisterLazySingleton<ChatPageViewModel>();
        Splatr.RegisterLazySingleton<ProfilePageViewModel>();
        Splatr.RegisterLazySingleton<WardrobePageViewModel>();
        Splatr.RegisterLazySingleton<FriendsPageViewModel>();
        Splatr.RegisterLazySingleton<InventoryPageViewModel>();
        Splatr.RegisterLazySingleton<RoomPageViewModel>();
        Splatr.RegisterLazySingleton<NavigatorPageViewModel>();
        Splatr.RegisterLazySingleton<GameDataPageViewModel>();
        Splatr.RegisterLazySingleton<InfoPageViewModel>();
        Splatr.RegisterLazySingleton<SettingsPageViewModel>();

        Splatr.RegisterLazySingleton<InventoryViewModel>();
        Splatr.RegisterLazySingleton<RoomInfoViewModel>();
        Splatr.RegisterLazySingleton<RoomAvatarsViewModel>();
        Splatr.RegisterLazySingleton<RoomVisitorsViewModel>();
        Splatr.RegisterLazySingleton<RoomBansViewModel>();
        Splatr.RegisterLazySingleton<RoomFurniViewModel>();
        Splatr.RegisterLazySingleton<RoomGiftsViewModel>();

        Splatr.RegisterLazySingleton<FurniDataViewModel>();
        Splatr.RegisterLazySingleton<ExternalTextsViewModel>();
        Splatr.RegisterLazySingleton<ExternalVariablesViewModel>();

        Splatr.RegisterLazySingleton<OfferItemsViewModel>();
    }

    static void RegisterViews()
    {
        Splatr.RegisterLazySingleton<MainWindow>();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
}
