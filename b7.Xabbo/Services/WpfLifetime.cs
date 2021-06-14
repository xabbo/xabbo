using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Hosting;

namespace b7.Xabbo.Services
{
    public class WpfLifetime : IHostLifetime
    {
        private readonly IHostApplicationLifetime _hostLifetime;
        private readonly Application _application;

        public WpfLifetime(IHostApplicationLifetime hostLifetime, Application application)
        {
            _hostLifetime = hostLifetime;
            _application = application;

            _hostLifetime.ApplicationStopping.Register(() => application.Shutdown());
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _application.Shutdown();

            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            _application.Exit += (s, e) => _hostLifetime.StopApplication();

            _application.Dispatcher.Invoke(() =>
            {
                _application.MainWindow.Show();
            });

            return Task.CompletedTask;
        }
    }
}
