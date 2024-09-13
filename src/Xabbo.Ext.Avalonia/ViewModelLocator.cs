using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Splat;

using Xabbo.Extension;
using Xabbo.GEarth;
using Xabbo.Core.GameData;

using Xabbo.Ext.Avalonia.Services;
using Xabbo.Ext.Avalonia.ViewModels;
using Xabbo.Ext.Commands;
using Xabbo.Ext.Components;
using Xabbo.Ext.Configuration;
using Xabbo.Ext.Services;
using Xabbo.Ext.Core.Services;

using Splatr = Splat.SplatRegistrations;
using Xabbo.Core.Game;

namespace Xabbo.Ext.Avalonia;

public static class ViewModelLocator
{
    static ViewModelLocator()
    {
        var container = Locator.CurrentMutable;

        var configRoot = new ConfigurationBuilder().Build();
        Splatr.RegisterConstant<IConfiguration>(configRoot);

        // Services
        Splatr.RegisterLazySingleton<IApplicationManager, AvaloniaAppManager>();
        Splatr.RegisterLazySingleton<IUiContext, AvaloniaUiContext>();

        Splatr.RegisterLazySingleton<AppSessionManager>();

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
        Splatr.RegisterLazySingleton<RoomAvatarsViewModel>();
        Splatr.RegisterLazySingleton<RoomVisitorsViewModel>();
        Splatr.RegisterLazySingleton<RoomBansViewModel>();
        Splatr.RegisterLazySingleton<RoomFurniViewModel>();

        Splatr.RegisterLazySingleton<FurniDataViewModel>();

        // Logic
        var ext = new GEarthExtension(
            GEarthOptions.Default.WithAssemblyVersion() with
            {
                Name = "xabbo",
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
        Splatr.RegisterLazySingleton<AvatarOverlayComponent>();

        Splatr.RegisterLazySingleton<AntiHandItemComponent>();
        Splatr.RegisterLazySingleton<AntiHcGiftNotificationComponent>();
        Splatr.RegisterLazySingleton<AntiIdleComponent>();
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
        Splatr.RegisterLazySingleton<CommandManager>();

        #pragma warning disable SPLATDI006
        Splatr.RegisterLazySingleton<CommandModule, AppCommands>();
        Splatr.RegisterLazySingleton<CommandModule, EffectCommands>();
        Splatr.RegisterLazySingleton<CommandModule, FindFriendCommand>();
        Splatr.RegisterLazySingleton<CommandModule, FurniCommands>();
        Splatr.RegisterLazySingleton<CommandModule, InfoCommands>();
        Splatr.RegisterLazySingleton<CommandModule, ModerationCommands>();
        Splatr.RegisterLazySingleton<CommandModule, MoodCommands>();
        Splatr.RegisterLazySingleton<CommandModule, RoomCommands>();
        Splatr.RegisterLazySingleton<CommandModule, TurnCommand>();
        Splatr.RegisterLazySingleton<CommandModule, UserProfileCommands>();
        Splatr.RegisterLazySingleton<CommandModule, ClickThroughCommand>();
        #pragma warning restore SPLATDI006

        container.RegisterLazySingleton(() => Locator.Current.GetServices<CommandModule>());

        Splatr.SetupIOC();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
}
