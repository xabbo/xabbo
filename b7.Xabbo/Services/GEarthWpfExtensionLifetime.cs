using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Hosting;
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

            _hostAppLifetime.ApplicationStopping.Register(() => application.Shutdown());
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _application.Shutdown();

            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            _application.Exit += (s, e) => _hostAppLifetime.StopApplication();

            // Task.Run(() => _extension.RunAsync());

            _application.Dispatcher.Invoke(() =>
            {
                _application.MainWindow.Show();
            });

            return Task.CompletedTask;
        }
    }
}
