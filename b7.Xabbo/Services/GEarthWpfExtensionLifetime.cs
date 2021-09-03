using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Microsoft.Extensions.Hosting;

using Xabbo.Interceptor;
using Xabbo.GEarth;

using b7.Xabbo.Util;

namespace b7.Xabbo.Services
{
    public class GEarthWpfExtensionLifetime : IHostLifetime
    {
        private readonly IHostApplicationLifetime _hostAppLifetime;
        
        public GEarthExtension Extension { get; }
        public Application Application { get; }
        public Window Window => Application.MainWindow;

        public GEarthWpfExtensionLifetime(
            IHostApplicationLifetime hostAppLifetime,
            Application application,
            GEarthExtension extension)
        {
            _hostAppLifetime = hostAppLifetime;
            
            Application = application;

            Extension = extension;
            Extension.InterceptorConnectionFailed += OnInterceptorConnectionFailed;
            Extension.InterceptorConnected += OnInterceptorConnected;
            Extension.Clicked += OnExtensionClicked;
            Extension.InterceptorDisconnected += OnInterceptorDisconnected;

            _hostAppLifetime.ApplicationStopping.Register(() => application.Shutdown());
        }

        private void OnInterceptorConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            MessageBox.Show(
                $"Failed to connect to G-Earth on port {Extension.Options.Port}.", "xabbo",
                MessageBoxButton.OK, MessageBoxImage.Error
            );

            Application.Shutdown();
        }

        private void OnInterceptorConnected(object? sender, EventArgs e)
        {
            if (!Extension.Options.IsInstalledExtension)
            {
                Window.Show();
            }
        }

        private void OnExtensionClicked(object? sender, EventArgs e)
        {
            Window.Dispatcher.InvokeAsync(
                () =>
                {
                    WindowUtil.ActivateWindow(Window);

                    if (!Window.IsVisible)
                    {
                        Window.Show();
                    }

                    if (Window.WindowState == WindowState.Minimized)
                    {
                        Window.WindowState = WindowState.Normal;
                    }
                },
                DispatcherPriority.ApplicationIdle
            );
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (Extension.IsInterceptorConnected &&
                !string.IsNullOrEmpty(Extension.Options.Cookie))
            {
                e.Cancel = true;
                Application.MainWindow.Hide();
            }
        }

        private void OnInterceptorDisconnected(object? sender, DisconnectedEventArgs e)
        {
            Application.Shutdown();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Application.Shutdown();

            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            Application.MainWindow.Closing += MainWindow_Closing;
            Application.Exit += (s, e) => _hostAppLifetime.StopApplication();

            _ = Extension.RunAsync();

            return Task.CompletedTask;
        }
    }
}
