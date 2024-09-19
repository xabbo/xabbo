using Xabbo.Configuration;
using Xabbo.Extension;
using Xabbo.Messages.Flash;
using Xabbo.Services.Abstractions;

namespace Xabbo.Components;

[Intercept(~ClientType.Shockwave)]
public partial class AntiTypingComponent(IExtension extension, IConfigProvider<AppConfig> config) : Component(extension)
{
    private readonly IConfigProvider<AppConfig> _config = config;

    [InterceptOut(nameof(Out.StartTyping))]
    private void OnUserStartTyping(Intercept e)
    {
        if (_config.Value.General.AntiTyping) e.Block();
    }
}
