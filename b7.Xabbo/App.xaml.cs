using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MaterialDesignThemes.Wpf;

using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

using Xabbo.Messages;
using Xabbo.Interceptor;
using Xabbo.GEarth;

using Xabbo.Core.Game;
using Xabbo.Core.GameData;

using b7.Xabbo.View;
using b7.Xabbo.Components;
using b7.Xabbo.Commands;
using b7.Xabbo.Services;
using b7.Xabbo.Configuration;
using b7.Xabbo.Util;
using b7.Xabbo.View.Pages;

namespace b7.Xabbo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host = null!;

        private static readonly Dictionary<string, string> _switchMappings = new()
        {
            ["-s"] = "Xabbo:Interceptor:Service",
            ["-p"] = "Xabbo:Interceptor:Port",
            ["-c"] = "Xabbo:Interceptor:Cookie",
            ["-f"] = "Xabbo:Interceptor:File"
        };

        public App()
        {
            Dispatcher.UnhandledException += (s, e) => LogError($"Unhandled dispatcher exception: {e.Exception}");
        }

        private static void LogError(string message) => File.AppendAllLines(
            "error.log", new[] { $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}" }
        );

        private void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging) { }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            Type[] executingAssemblyTypes = Assembly.GetExecutingAssembly().GetTypes();

            // Options
            services.Configure<GameOptions>(context.Configuration.GetSection(GameOptions.Path));
            services.Configure<AntiBobbaOptions>(context.Configuration.GetSection(AntiBobbaOptions.Path));

            // Application
            services.AddSingleton(this);
            services.AddSingleton<Application>(this);
            services.AddSingleton<IHostLifetime, GEarthWpfExtensionLifetime>();
            services.AddSingleton(Dispatcher);
            services.AddSingleton<IUiContext, DispatcherContext>();

            services.AddSingleton<IOperationManager, OperationManager>();

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IPageService, PageService>();

            // UI
            services.AddSingleton<ISnackbarMessageQueue, SnackbarMessageQueue>();

            // Pages
            services.AddScoped<GeneralPage>();
            services.AddScoped<ProfilePage>();
            services.AddScoped<ChatPage>();
            services.AddScoped<RoomPage>();
            services.AddScoped<FriendsPage>();
            services.AddScoped<NavigatorPage>();
            services.AddScoped<GameDataPage>();
            services.AddScoped<DebugPage>();

            // Interceptor
            services.AddGEarthOptions(options => options
                .WithName("xabbo")
                .WithDescription("enhanced habbo")
                .WithAuthor("b7")
                .WithConfiguration(context.Configuration)
            );

            services.AddSingleton<IMessageManager, UnifiedMessageManager>();

            services.AddSingleton<GEarthExtension>();
            services.AddSingleton<IInterceptor>(x => x.GetRequiredService<GEarthExtension>());
            services.AddSingleton<IRemoteInterceptor>(x => x.GetRequiredService<GEarthExtension>());

            // Web
            services.AddHttpClient("habbo")
                .ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.Add(
                        "User-Agent",
                        context.Configuration.GetValue<string>("Web:UserAgent")
                    );
                });

            services.AddSingleton<IUriProvider<HabboEndpoints>, HabboUriProvider>();
            services.AddSingleton<IGameDataManager, GameDataManager>();

            // Game state
            foreach (Type gameStateManagerType in GameStateManager.GetManagerTypes())
            {
                services.AddSingleton(gameStateManagerType);
            }

            // Commands
            services.AddSingleton<CommandManager>();
            foreach (var type in executingAssemblyTypes
                .Where(x =>
                    x.Namespace == "b7.Xabbo.Commands" &&
                    x.IsAssignableTo(typeof(CommandModule)) &&
                    !x.IsAbstract
                ))
            {
                services.AddSingleton(typeof(CommandModule), type);
            }

            // Components
            foreach (Type type in executingAssemblyTypes)
            {
                if (!type.IsAbstract &&
                    typeof(Component).IsAssignableFrom(type))
                {
                    Debug.WriteLine($"Registering component service: {type.Name}");
                    services.AddSingleton(type);
                    services.AddSingleton(typeof(Component), provider => provider.GetRequiredService(type));
                }
            }

            // Wardrobe
            services.AddSingleton<IWardrobeRepository, LiteDbWardrobeRepository>();

            // View managers
            foreach (Type type in executingAssemblyTypes)
            {
                if (type.Namespace == "b7.Xabbo.ViewModel" &&
                    type.Name.EndsWith("ViewManager"))
                {
                    Debug.WriteLine($"Registering view manager: {type.Name}");
                    services.AddSingleton(type);
                }
            }

            // Views
            services.AddSingleton<MainWindow>();

            // Hosted services
            services.AddHostedService<HotelResourceManager>();

            services.AddHostedService(provider => provider.GetRequiredService<XabbotComponent>());
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                _host = Host.CreateDefaultBuilder(e.Args)
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        IConfigurationRoot originalConfig = config.Build();

                        config.Sources.Clear();
                        config.AddJsonFile("appsettings.json", false, true);

                        string domain = originalConfig.GetValue<string>("Web:Domain");
                        if (!string.IsNullOrWhiteSpace(domain))
                        {
                            config.AddJsonFile($"appsettings.{domain}.json", true);
                        }

                        config.AddEnvironmentVariables();
                        config.AddCommandLine(e.Args, _switchMappings);
                    })
                    .ConfigureLogging(ConfigureLogging)
                    .ConfigureServices(ConfigureServices)
                    .Build();

                MainWindow = _host.Services.GetRequiredService<MainWindow>();

                await _host.StartAsync();
            }
            catch (Exception ex)
            {
                LogError($"Initialization failed: {ex}");
                MessageBox.Show(
                    $"Initialization failed: {ex.Message}", "xabbo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }

            base.OnExit(e);
        }
    }
}
