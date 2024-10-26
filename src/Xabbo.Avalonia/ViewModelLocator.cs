using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Splat;
using Splat.Microsoft.Extensions.Logging;
using Avalonia.Logging;
using Avalonia.Controls.ApplicationLifetimes;
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
        var container = Locator.CurrentMutable;

        // Settings
        Splatr.RegisterLazySingleton<IAppPathProvider, AppPathService>();
        Splatr.RegisterLazySingleton<IConfigProvider<AppConfig>, AppConfigProvider>();

        // Static configuration
        var configRoot = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        Splatr.RegisterConstant<IConfiguration>(configRoot);

        // Logging
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => {
            builder
                .AddConfiguration(configRoot.GetSection("Logging"))
                .AddSimpleConsole(console =>
                {
                    console.SingleLine = true;
                    console.IncludeScopes = true;
                    console.TimestampFormat = "[HH:mm:ss.fff] ";
                });
        });
        container.RegisterConstant(loggerFactory);
        container.UseMicrosoftExtensionsLoggingWithWrappingFullLogger(loggerFactory);

        Logger.Sink = new AvaloniaLogSink(loggerFactory);

        // Application services
        {
            Lazy<IHostApplicationLifetime> lazy = new Lazy<IHostApplicationLifetime>(
                () => new AvaloniaHostApplicationLifetime(
                    Locator.Current.GetService<ILoggerFactory>()!,
                    Locator.Current.GetService<IApplicationLifetime>()!
                )
            );
            container.Register<Lazy<IHostApplicationLifetime>>(() => lazy);
            container.Register<IHostApplicationLifetime>(() => {
                return lazy.Value;
            });
        }

        Splatr.RegisterLazySingleton<IApplicationManager, XabboAppManager>();
        Splatr.RegisterLazySingleton<IUiContext, AvaloniaUiContext>();
        Splatr.RegisterLazySingleton<IClipboardService, ClipboardService>();
        Splatr.RegisterLazySingleton<ILauncherService, LauncherService>();
        container.RegisterLazySingleton(() => (IDialogService)new DialogService(
            new DialogManager(
                viewLocator: new ViewLocator(),
                dialogFactory: new DialogFactory().AddFluent(FluentMessageBoxType.ContentDialog)),
            viewModelFactory: x => Locator.Current.GetService(x)));

        Splatr.RegisterLazySingleton<GlobalExceptionHandler>();

        // ViewModels
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

        // Logic
        Splatr.RegisterConstant(new GEarthOptions() with
        {
            Name = "xabbo",
            Description = "enhanced habbo",
            Author = "b7"
        });
        Splatr.RegisterLazySingleton<GEarthExtension>();
        container.Register<IExtension>(() => Locator.Current.GetService<GEarthExtension>());
        container.Register<IRemoteExtension>(() => Locator.Current.GetService<GEarthExtension>());
        container.Register<IInterceptor>(() => Locator.Current.GetService<GEarthExtension>());

        // Xabbo services
        Splatr.RegisterLazySingleton<IGameStateService, GameStateService>();
        Splatr.RegisterLazySingleton<IFigureConverterService, FigureConverterService>();
        Splatr.RegisterLazySingleton<IWardrobeRepository, JsonWardrobeRepository>();
        Splatr.RegisterLazySingleton<IHabboApi, HabboApi>();

        Splatr.RegisterLazySingleton<FurniPlacementController>();
        Splatr.RegisterLazySingleton<IPlacementFactory, PlacementFactory>();

        // Xabbo core components
        Splatr.RegisterLazySingleton<ProfileManager>();
        Splatr.RegisterLazySingleton<InventoryManager>();
        Splatr.RegisterLazySingleton<RoomManager>();
        Splatr.RegisterLazySingleton<TradeManager>();
        Splatr.RegisterLazySingleton<FriendManager>();

        Splatr.RegisterLazySingleton<IGameDataManager, GameDataManager>();

        // Xabbo components
        Splatr.RegisterLazySingleton<IOperationManager, OperationManager>();

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

        // Controllers
        Splatr.RegisterLazySingleton<ControllerInitializer>();

        Splatr.RegisterLazySingleton<ClickToController>();
        Splatr.RegisterLazySingleton<RoomRightsController>();
        Splatr.RegisterLazySingleton<RoomModerationController>();
        Splatr.RegisterLazySingleton<RoomFurniController>();
        Splatr.RegisterLazySingleton<PrivacyController>();

        // Xabbo commands
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

        container.RegisterLazySingleton(() => Locator.Current.GetServices<CommandModule>());

        // Views
        Splatr.RegisterLazySingleton<MainWindow>();

        Splatr.SetupIOC();
    }

    public static MainViewModel Main => Locator.Current.GetService<MainViewModel>()!;
}
