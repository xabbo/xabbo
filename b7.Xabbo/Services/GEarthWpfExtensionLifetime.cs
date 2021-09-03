using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Hosting;

using Xabbo.Interceptor;
using Xabbo.GEarth;

namespace b7.Xabbo.Services
{
    public class GEarthWpfExtensionLifetime : IHostLifetime
    {
        private readonly IHostApplicationLifetime _hostAppLifetime;
        private readonly Application _application;

        private readonly GEarthExtension _extension;

        public GEarthWpfExtensionLifetime(IHostApplicationLifetime hostAppLifetime, Application application,
            GEarthExtension extension)
        {
            _hostAppLifetime = hostAppLifetime;
            _application = application;

            _extension = extension;
            _extension.InterceptorConnectionFailed += OnInterceptorConnectionFailed;
            _extension.InterceptorConnected += OnInterceptorConnected;
            _extension.Clicked += OnExtensionClicked;
            _extension.InterceptorDisconnected += OnInterceptorDisconnected;

            _hostAppLifetime.ApplicationStopping.Register(() => application.Shutdown());
        }

        private void OnInterceptorConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            MessageBox.Show(
                $"Failed to connect to G-Earth on port {_extension.Options.Port}.", "xabbo",
                MessageBoxButton.OK, MessageBoxImage.Error
            );

            _application.Shutdown();
        }

        private void OnInterceptorConnected(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_extension.Options.Cookie))
            {
                _application.MainWindow.Show();
            }
        }

        private void OnExtensionClicked(object? sender, EventArgs e)
        {
            if (_application.MainWindow.IsVisible)
            {
                if (_application.MainWindow.WindowState == WindowState.Minimized)
                {
                    _application.MainWindow.WindowState = WindowState.Normal;
                }

                _application.MainWindow.Activate();
                _application.MainWindow.Focus();
            }
            else
            {
                _application.MainWindow.Show();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (_extension.IsInterceptorConnected &&
                !string.IsNullOrEmpty(_extension.Options.Cookie))
            {
                e.Cancel = true;
                _application.MainWindow.Hide();
            }
        }

        private void OnInterceptorDisconnected(object? sender, DisconnectedEventArgs e)
        {
            _application.Shutdown();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _application.Shutdown();

            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            _application.MainWindow.Closing += MainWindow_Closing;
            _application.Exit += (s, e) => _hostAppLifetime.StopApplication();

            return Task.CompletedTask;
        }
    }
}
