using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Xabbo.Ext.Avalonia.Services;

internal static class HostingExtensions
{
    public static IHostBuilder UseAvaloniaLifetime(this IHostBuilder host)
    {
        return host.ConfigureServices(services => services.AddSingleton<IHostLifetime, AvaloniaLifetime>());
    }
}
