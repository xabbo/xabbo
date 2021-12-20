using System;
using System.IO;
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
            Application.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Failed to connect to G-Earth on port {Extension.Options.Port}.", "xabbo",
                    MessageBoxButton.OK, MessageBoxImage.Error
                );

                Application.Shutdown();
            });
        }

        private void OnInterceptorConnected(object? sender, EventArgs e)
        {
            if (!Extension.Options.IsInstalledExtension)
            {
                Application.Dispatcher.InvokeAsync(() => Window.Show());
            }
        }

        private void OnExtensionClicked(object? sender, EventArgs e)
        {
            Application.Dispatcher.InvokeAsync(
                () => WindowUtil.Show(Window),
                DispatcherPriority.ApplicationIdle
            );
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (Extension.IsInterceptorConnected)
            {
                e.Cancel = true;
                Window.Hide();
            }
        }

        private void OnInterceptorDisconnected(object? sender, DisconnectedEventArgs e)
        {
            Application.Dispatcher.Invoke(() => Application.Shutdown());
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Application.Shutdown();

            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            Window.Closing += OnWindowClosing;
            Application.Exit += (s, e) => _hostAppLifetime.StopApplication();

            _ = Task.Run(RunExtensionAsync, cancellationToken);

            return Task.CompletedTask;
        }

        private async Task RunExtensionAsync()
        {
            try
            {
                await Extension.RunAsync();
            }
            catch (Exception ex)
            {
                File.AppendAllText(
                    "error.log",
                    $"\r\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [Extension.RunAsync] {ex}"
                );
            }
        }
    }
}
