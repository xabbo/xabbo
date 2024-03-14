using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace b7.Xabbo.Avalonia.Services;

internal static class HostingExtensions
{
    public static IHostBuilder UseAvaloniaLifetime(this IHostBuilder host)
    {
        return host.ConfigureServices(services => services.AddSingleton<IHostLifetime, AvaloniaLifetime>());
    }
}
