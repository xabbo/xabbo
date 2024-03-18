using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Splat;

using Xabbo.Extension;
using Xabbo.GEarth;
using Xabbo.Core.GameData;

using b7.Xabbo.Avalonia.Services;
using b7.Xabbo.Avalonia.ViewModels;
using b7.Xabbo.Commands;
using b7.Xabbo.Components;
using b7.Xabbo.Configuration;
using b7.Xabbo.Services;

namespace b7.Xabbo.Avalonia;

public static class ViewModelLocator
{
    static ViewModelLocator()
    {
        var container = Locator.CurrentMutable;

        var configRoot = new ConfigurationBuilder().Build();
        Splatr.RegisterConstant<IConfiguration>(configRoot);

        // Services
        Splatr.RegisterLazySingleton<IUiContext, AvaloniaUiContext>();

        // ViewModels
        Splatr.RegisterLazySingleton<MainViewModel>();

        // Pages
        Splatr.RegisterLazySingleton<GeneralPageViewModel>();
        Splatr.RegisterLazySingleton<ChatPageViewModel>();
        Splatr.RegisterLazySingleton<ProfilePageViewModel>();
        Splatr.RegisterLazySingleton<FriendsPageViewModel>();
        Splatr.RegisterLazySingleton<RoomPageViewModel>();
        Splatr.RegisterLazySingleton<NavigatorPageViewModel>();
        Splatr.RegisterLazySingleton<GameDataPageViewModel>();

        Splatr.RegisterLazySingleton<RoomInfoViewModel>();
        Splatr.RegisterLazySingleton<RoomEntitiesViewModel>();
        Splatr.RegisterLazySingleton<RoomVisitorsViewModel>();
        Splatr.RegisterLazySingleton<RoomBansViewModel>();
        Splatr.RegisterLazySingleton<RoomFurniViewModel>();

        Splatr.RegisterLazySingleton<FurniDataViewModel>();

        // Logic
        var ext = new GEarthExtension(
            GEarthOptions.Default.WithAssemblyVersion() with
            {
                Title = "xabbo",
                Description = "enhanced habbo",
                Author = "b7"
            }
        );
        Splatr.RegisterConstant(ext);
        Splatr.RegisterConstant<IExtension>(ext);
        Splatr.RegisterLazySingleton<GEarthExtensionLifetime>();

        // Configuration
        container.RegisterLazySingleton(() => Options.Create(new GameOptions()));

        container.RegisterConstant<ILoggerFactory>(new LoggerFactory());

        // Xabbo core components
        Splatr.RegisterLazySingleton<GameStateService>();
        container.Register(() => Locator.Current.GetService<GameStateService>()!.Profile);
        container.Register(() => Locator.Current.GetService<GameStateService>()!.Trade);
        container.Register(() => Locator.Current.GetService<GameStateService>()!.Friends);
        container.Register(() => Locator.Current.GetService<GameStateService>()!.Inventory);
        container.Register(() => Locator.Current.GetService<GameStateService>()!.Room);

        container.RegisterLazySingleton<IGameDataManager>(() => new GameDataManager());

        // Xabbo components
        Splatr.RegisterLazySingleton<XabbotComponent>();
        Splatr.RegisterLazySingleton<NotificationComponent>();
        // Splatr.RegisterLazySingleton<FlashWindowComponent>();
        Splatr.RegisterLazySingleton<EntityOverlayComponent>();
        // Splatr.RegisterLazySingleton<DisconnectionReasonComponent>();
        Splatr.RegisterLazySingleton<AutoClaimComponent>();

        Splatr.RegisterLazySingleton<AntiHandItemComponent>();
        Splatr.RegisterLazySingleton<AntiHcGiftNotificationComponent>();
        Splatr.RegisterLazySingleton<AntiIdleComponent>();
        // Splatr.RegisterLazySingleton<AntiKickComponent>();
        Splatr.RegisterLazySingleton<AntiTradeComponent>();
        Splatr.RegisterLazySingleton<AntiTurnComponent>();
        Splatr.RegisterLazySingleton<AntiTypingComponent>();
        Splatr.RegisterLazySingleton<AntiWalkComponent>();
        Splatr.RegisterLazySingleton<ClickThroughComponent>();
        Splatr.RegisterLazySingleton<ClickToComponent>();
        Splatr.RegisterLazySingleton<RespectedComponent>();

        Splatr.RegisterLazySingleton<ChatComponent>();
        Splatr.RegisterLazySingleton<DoorbellComponent>();
        Splatr.RegisterLazySingleton<FlattenRoomComponent>();
        Splatr.RegisterLazySingleton<FurniActionsComponent>();
        Splatr.RegisterLazySingleton<RoomEntryComponent>();
        Splatr.RegisterLazySingleton<RoomModeratorComponent>();

        // Xabbo commands
        // container.Register<IOperationManager, OperationManager>();
        Splatr.RegisterLazySingleton<CommandManager>();

        // Splatr.RegisterLazySingleton<AppCommands>();
        Splatr.RegisterLazySingleton<CommandModule, EffectCommands>();
        Splatr.RegisterLazySingleton<FindFriendCommand>();
        Splatr.RegisterLazySingleton<FurniCommands>();
        Splatr.RegisterLazySingleton<InfoCommands>();
        Splatr.RegisterLazySingleton<ModerationCommands>();
        Splatr.RegisterLazySingleton<MoodCommands>();
        // Splatr.RegisterLazySingleton<OperationCommands>();
        Splatr.RegisterLazySingleton<RoomCommands>();
        Splatr.RegisterLazySingleton<SpoofChatCommands>();
        Splatr.RegisterLazySingleton<TurnCommand>();
        Splatr.RegisterLazySingleton<UserProfileCommands>();

        container.RegisterLazySingleton(() => Locator.Current.GetServices<CommandModule>());

        Splatr.SetupIOC();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
}
