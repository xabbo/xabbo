using System;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Splat;
using Splat.Microsoft.Extensions.Logging;

using Avalonia.Controls.ApplicationLifetimes;

using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;

using Xabbo.Interceptor;
using Xabbo.Extension;
using Xabbo.GEarth;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;

using Xabbo.Ext.Avalonia.Services;
using Xabbo.Ext.Avalonia.ViewModels;
using Xabbo.Ext.Commands;
using Xabbo.Ext.Components;
using Xabbo.Ext.Configuration;
using Xabbo.Ext.Services;
using Xabbo.Ext.Core.Services;

using Splatr = Splat.SplatRegistrations;

using IHostApplicationLifetime = Microsoft.Extensions.Hosting.IHostApplicationLifetime;

namespace Xabbo.Ext.Avalonia;

public static class ViewModelLocator
{
    static ViewModelLocator()
    {
        var container = Locator.CurrentMutable;

        var configRoot = new ConfigurationBuilder().Build();
        Splatr.RegisterConstant<IConfiguration>(configRoot);

        // Configuration
        container.RegisterLazySingleton(() => Options.Create(new GameOptions()));

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => {
            builder.AddSimpleConsole(config =>
            {
                config.IncludeScopes = true;
            });
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
        });
        container.RegisterConstant(loggerFactory);
        container.UseMicrosoftExtensionsLoggingWithWrappingFullLogger(loggerFactory);

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

        Splatr.RegisterLazySingleton<IApplicationManager, AvaloniaAppManager>();
        Splatr.RegisterLazySingleton<IUiContext, AvaloniaUiContext>();
        container.RegisterLazySingleton(() => (IDialogService)new DialogService(
            new DialogManager(
                viewLocator: new ViewLocator(),
                dialogFactory: new DialogFactory().AddFluent(FluentMessageBoxType.ContentDialog)),
            viewModelFactory: x => Locator.Current.GetService(x)));

        Splatr.RegisterLazySingleton<AppSessionManager>();
        Splatr.RegisterLazySingleton<GlobalExceptionHandler>();

        // ViewModels
        Splatr.RegisterLazySingleton<MainViewModel>();

        // Pages
        Splatr.RegisterLazySingleton<GeneralPageViewModel>();
        Splatr.RegisterLazySingleton<ChatPageViewModel>();
        Splatr.RegisterLazySingleton<ProfilePageViewModel>();
        Splatr.RegisterLazySingleton<WardrobePageViewModel>();
        Splatr.RegisterLazySingleton<FriendsPageViewModel>();
        Splatr.RegisterLazySingleton<RoomPageViewModel>();
        Splatr.RegisterLazySingleton<NavigatorPageViewModel>();
        Splatr.RegisterLazySingleton<GameDataPageViewModel>();
        Splatr.RegisterLazySingleton<SettingsPageViewModel>();

        Splatr.RegisterLazySingleton<RoomInfoViewModel>();
        Splatr.RegisterLazySingleton<RoomAvatarsViewModel>();
        Splatr.RegisterLazySingleton<RoomVisitorsViewModel>();
        Splatr.RegisterLazySingleton<RoomBansViewModel>();
        Splatr.RegisterLazySingleton<RoomFurniViewModel>();

        Splatr.RegisterLazySingleton<FurniDataViewModel>();

        // Logic
        Splatr.RegisterConstant(GEarthOptions.Default.WithAssemblyVersion() with
        {
            Name = "xabbo",
            Description = "enhanced habbo",
            Author = "b7"
        });
        Splatr.RegisterLazySingleton<GEarthExtension>();
        container.Register<IExtension>(() => Locator.Current.GetService<GEarthExtension>());
        container.Register<IInterceptor>(() => Locator.Current.GetService<GEarthExtension>());
        Splatr.RegisterLazySingleton<GEarthExtensionLifetime>();

        // Xabbo services
        Splatr.RegisterLazySingleton<IGameStateService, GameStateService>();
        Splatr.RegisterLazySingleton<IFigureConverterService, FigureConverterService>();
        Splatr.RegisterLazySingleton<IWardrobeRepository, JsonWardrobeRepository>();

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
        Splatr.RegisterLazySingleton<CommandModule, OperationCommands>();
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
