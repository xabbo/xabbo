using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Diagnostics;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xabbo.Messages;
using Xabbo.Interceptor;
using Xabbo.GEarth;

using Xabbo.Core.Game;

using b7.Xabbo.View;
using b7.Xabbo.Components;
using b7.Xabbo.Commands;
using b7.Xabbo.Services;
using b7.Xabbo.Configuration;

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
            ["-p"] = "Interceptor:Port"
        };

        public App() { }

        private void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging) { }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            Type[] executingAssemblyTypes = Assembly.GetExecutingAssembly().GetTypes();

            // Options
            services.Configure<GameOptions>(context.Configuration.GetSection(GameOptions.Path));

            // Application
            services.AddSingleton<Application>(this);
            services.AddSingleton<IHostLifetime, WpfLifetime>();
            services.AddSingleton(Dispatcher);
            services.AddSingleton<IUiContext, DispatcherContext>();

            // Interceptor
            services.AddSingleton(new GEarthOptions
            {
                Title = "xabbo",
                Author = "b7",
                Description = "enhanced habbo",
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?"
            });

            services.AddSingleton<IMessageManager, UnifiedMessageManager>();

            // Web
            services.AddHttpClient("Xabbo")
                .ConfigureHttpClient(client =>
                {
                    client.DefaultRequestHeaders.Add(
                        "User-Agent",
                        context.Configuration.GetValue<string>("Web:UserAgent")
                    );
                });

            services.AddSingleton<IUriProvider<HabboEndpoints>, HabboUrlProvider>();
            services.AddSingleton<IGameDataManager, GameDataManager>();

            // Interceptor
            services.AddSingleton<GEarthExtension>();
            services.AddSingleton<IInterceptor>(x => x.GetRequiredService<GEarthExtension>());
            services.AddSingleton<IRemoteInterceptor>(x => x.GetRequiredService<GEarthExtension>());

            // Game state
            foreach (Type gameStateManagerType in GameStateManager.GetManagerTypes())
            {
                services.AddSingleton(gameStateManagerType);
            }

            // Commands
            services.AddSingleton<CommandManager>();

            // Components
            foreach (Type type in executingAssemblyTypes)
            {
                if (!type.IsAbstract &&
                    typeof(Component).IsAssignableFrom(type))
                {
                    Debug.WriteLine($"Registering component service: {type.Name}");
                    services.AddSingleton(type);
                }
            }

            services.AddHostedService(provider => provider.GetRequiredService<XabboUserComponent>());

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

            // Initialiation
            services.AddHostedService<InitializationService>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder(e.Args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    IConfigurationRoot originalConfig = config.Build();

                    config.Sources.Clear();
                    config.AddJsonFile("appsettings.json");

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

            _host.Services.GetRequiredService<IUriProvider<HabboEndpoints>>();

            MainWindow = _host.Services.GetRequiredService<MainWindow>();

            await _host.StartAsync();
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
