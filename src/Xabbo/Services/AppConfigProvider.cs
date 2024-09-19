using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xabbo.Serialization;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;
using Xabbo.Models.Enums;

namespace Xabbo.Services;

public sealed partial class AppConfigProvider(
    IAppPathProvider appPathService,
    IHostApplicationLifetime lifetime,
    ILoggerFactory? loggerFactory = null
)
    : ConfigProvider<AppConfig>(JsonSourceGenerationContext.Default.AppConfig, lifetime, loggerFactory)
{
    protected override string FilePath => appPathService.GetPath(AppPathKind.Settings);
}
