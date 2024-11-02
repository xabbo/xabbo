using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Humanizer;

using Xabbo.Messages;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Services.Abstractions;
using Xabbo.ViewModels;
using Xabbo.Utility;
using Xabbo.Avalonia.Views;

using IApplicationLifetime = Avalonia.Controls.ApplicationLifetimes.IApplicationLifetime;

namespace Xabbo.Avalonia.Services;

public sealed class XabboAppManager : IApplicationManager
{
    private readonly ILogger _logger;

    private readonly IHostApplicationLifetime _hostLifetime;
    private readonly IApplicationLifetime _appLifetime;

    private readonly Application _application;
    private readonly IUiContext _uiContext;
    private readonly IRemoteExtension _extension;
    private readonly IGameDataManager _gameDataManager;
    private readonly ProfileManager _profileManager;

    private readonly MainViewModel _mainViewModel;
    private readonly Lazy<MainWindow> _mainWindow;

    private DisconnectReason _currentDisconnectReason = DisconnectReason.Unknown;

    private readonly CancellationTokenSource _extensionStopTokenSource;
    private readonly TaskCompletionSource _tcsApplicationError = new();
    private readonly SemaphoreSlim _errorSemaphore = new(1, 1);

    private Session _lastSession = Session.None;

    private bool _isRunning = false;

    public XabboAppManager(
        ILoggerFactory loggerFactory,
        IHostApplicationLifetime hostLifetime,
        IApplicationLifetime appLifetime,
        Application application,
        MainViewModel mainViewModel,
        Lazy<MainWindow> mainWindow,
        IUiContext uiContext,
        IRemoteExtension extension,
        IGameDataManager gameDataManager,
        ProfileManager profileManager
    )
    {
        _logger = loggerFactory.CreateLogger<XabboAppManager>();

        _hostLifetime = hostLifetime;
        _appLifetime = appLifetime;
        _application = application;
        _mainViewModel = mainViewModel;
        _mainWindow = mainWindow;
        _uiContext = uiContext;

        _extension = extension;
        _gameDataManager = gameDataManager;
        _profileManager = profileManager;

        _extension.Connected += OnConnected;
        _extension.Disconnected += OnDisconnected;

        _extensionStopTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_hostLifetime.ApplicationStopping);

        _extension.Activated += OnActivated;

        _hostLifetime.ApplicationStarted.Register(OnApplicationStarted);
    }

    private void OnApplicationStarted() => Task.Run(async () => {
        try
        {
            await RunExtensionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in extension runner.");
        }
    });

    private void OnActivated() => BringToFront();

    private void HandleApplicationException(Exception ex)
    {
        _logger.LogError(ex, "Application exception occurred.");

        if (!_errorSemaphore.Wait(0))
            return;

        string errorMessage = ex.Message;

        Header header = Header.Unknown;
        string? messageName = null;

        if (ex is UnhandledInterceptException iex)
        {
            header = iex.Header;
            string direction = iex.Header.Direction switch
            {
                Direction.In => "incoming",
                Direction.Out => "outgoing",
                _ => "unknown"
            };
            string headerText = $"{direction} header {iex.Header.Value}";
            string handlerText = headerText;
            if (_extension.Messages.TryGetNames(iex.Header, out var names) &&
                names.GetName(_lastSession.Client.Type) is string name)
            {
                messageName = name;
                handlerText = $"{direction} message '{messageName}'";
            }

            errorMessage = $"An error occurred in handler for {handlerText}.";
            ex = iex.InnerException ?? ex;
        }

        List<string?> errorDetails = [
            $"xabbo {Assembly.GetEntryAssembly().GetVersionString()}",
            ""
        ];

        if (_lastSession != Session.None)
        {
            errorDetails.AddRange([
                $"Session details:",
                $"  Hotel: {_lastSession.Hotel.Name}",
                $"  Client type: {_lastSession.Client.Type}",
                $"  Client identifier: {_lastSession.Client.Identifier}",
                $"  Client version: {_lastSession.Client.Version}",
                ""
            ]);
        }

        if (header != Header.Unknown)
        {
            errorDetails.AddRange([
                "Message details:",
                $"  Direction: {header.Direction}",
                $"  Header value: {header.Value}"
            ]);
            if (!string.IsNullOrWhiteSpace(messageName))
                errorDetails.Add($"  Name: {messageName}");
            errorDetails.Add("");
        }

        errorDetails.AddRange([ex.Message, ex.StackTrace]);

        _uiContext.Invoke(() => {
            _application.Resources["AppStatus"] = "An error occurred.";
            _application.Resources["IsError"] = true;
            _application.Resources["IsConnecting"] = false;
            _application.Resources["IsConnected"] = false;

            _mainViewModel.AppError = string.Join("\n", errorDetails);
        });
    }

    public async Task RunExtensionAsync()
    {
        bool error = false;

        try
        {
            _logger.LogInformation("Connecting to G-Earth.");

            _isRunning = true;
            await await Task.WhenAny(_extension.RunAsync(_hostLifetime.ApplicationStopping), _tcsApplicationError.Task);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            error = true;

            HandleApplicationException(ex);
        }
        finally
        {
            _isRunning = false;

            if (!error)
            {
                _uiContext.Invoke(() =>
                {
                    _application.Resources["IsConnecting"] = false;
                    _application.Resources["IsConnected"] = false;
                    _application.Resources["AppStatus"] = "Connection to G-Earth has ended.\nxabbo will now shut down.";
                });
                await Task.Delay(5000);
                _hostLifetime.StopApplication();
            }
            else
            {
                BringToFront();
            }
        }
    }

    private Window? GetMainWindow()
    {
        Window? window = null;

        if (_appLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            if (desktopLifetime.MainWindow is null)
            {
                Dispatcher.UIThread.Invoke(() => {
                    window = _mainWindow.Value;
                    window.DataContext = Application.Current?.DataContext;
                    window.Closing += OnMainWindowClosing;
                    desktopLifetime.MainWindow = window;
                });
            }
            else
            {
                window = _mainWindow.Value;
            }
        }

        return window;
    }

    public void BringToFront()
    {
        Dispatcher.UIThread.Invoke(() => {
            var window = GetMainWindow();
            if (window is not null)
            {
                window.Show();
                window.Activate();
            }
        });
    }

    private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (e.CloseReason is not WindowCloseReason.WindowClosing)
            return;

        if (!_isRunning)
        {
            _hostLifetime.StopApplication();
        }
        else
        {
            e.Cancel = true;
            if (sender is Window window)
                window.Hide();
        }
    }

    public void FlashWindow() { }

    private void SetStatus(string status)
    {
        _uiContext.Invoke(() => _application.Resources["AppStatus"] = status);
    }

    private void OnConnected(ConnectedEventArgs e)
    {
        _lastSession = e.Session;
        _currentDisconnectReason = DisconnectReason.Unknown;
        _extension.Intercept<DisconnectReasonMsg>(HandleDisconnectReason);

        _uiContext.Invoke(() =>
        {
            _application.Resources["IsConnecting"] = false;
            _application.Resources["IsConnected"] = true;
            _application.Resources["IsOrigins"] = e.Session.Is(ClientType.Origins);
            _application.Resources["IsModern"] = !e.Session.Is(ClientType.Origins);
        });

        CancellationToken ct = _extension.DisconnectToken;
        Task.Run(() => InitializeAsync(e.Session, ct));
    }

    private async Task InitializeAsync(Session session, CancellationToken ct)
    {
        try
        {
            SetStatus($"Loading game data for {session.Hotel.Name} hotel...");
            await _gameDataManager.WaitForLoadAsync(ct);

            SetStatus($"Waiting for user data...");
            await _profileManager.GetUserDataAsync();

            _uiContext.Invoke(() =>
            {
                _application.Resources["IsReady"] = true;
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _tcsApplicationError.TrySetException(ex);
        }
    }

    void HandleDisconnectReason(DisconnectReasonMsg msg)
    {
        _currentDisconnectReason = msg.Reason;
    }

    private void OnDisconnected()
    {
        _uiContext.Invoke(() =>
        {
            _application.Resources["AppStatus"] = _currentDisconnectReason switch
            {
                DisconnectReason.Unknown or
                DisconnectReason.Disconnected => "Disconnected!\nWaiting for another connection...",
                _ => $"Disconnected! {_currentDisconnectReason.Humanize()}.\nWaiting for another connection..."
            };
            _application.Resources["IsConnecting"] = true;
            _application.Resources["IsConnected"] = false;
            _application.Resources["IsReady"] = false;
        });
    }
}