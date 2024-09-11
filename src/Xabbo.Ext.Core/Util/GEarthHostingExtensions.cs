using Microsoft.Extensions.DependencyInjection;

using Xabbo.Interceptor;
using Xabbo.Extension;
using Xabbo.GEarth;

namespace Xabbo.Ext.Util;

public static class GEarthHostingExtensions
{
    public static IServiceCollection AddRemoteExtension<TExtension>(this IServiceCollection services)
        where TExtension : class, IRemoteExtension =>
            services
                .AddSingleton<TExtension>()
                .AddSingleton<IInterceptor>(x => x.GetRequiredService<TExtension>())
                .AddSingleton<IExtension>(x => x.GetRequiredService<TExtension>())
                .AddSingleton<IRemoteExtension>(x => x.GetRequiredService<TExtension>());

    public static IServiceCollection AddGEarthOptions(this IServiceCollection services, Func<GEarthOptions, GEarthOptions> configure)
        => services.AddSingleton(configure(GEarthOptions.Default));
}
