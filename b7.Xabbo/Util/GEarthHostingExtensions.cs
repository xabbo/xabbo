using System;

using Microsoft.Extensions.DependencyInjection;

using Xabbo.GEarth;

namespace b7.Xabbo.Util
{
    public static class GEarthHostingExtensions
    {
        public static IServiceCollection AddGEarthOptions(this IServiceCollection services, Func<GEarthOptions, GEarthOptions> configure)
            => services.AddSingleton(configure(GEarthOptions.Default));
    }
}
