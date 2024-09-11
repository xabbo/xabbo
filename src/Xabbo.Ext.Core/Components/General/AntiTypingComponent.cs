using Microsoft.Extensions.Configuration;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Messages.Flash;

namespace Xabbo.Ext.Components;

[Intercept(~ClientType.Shockwave)]
public partial class AntiTypingComponent : Component
{
    public AntiTypingComponent(IExtension extension,
        IConfiguration config)
        : base(extension)
    {
        IsActive = config.GetValue("AntiTyping:Active", true);
    }

    [InterceptOut(nameof(Out.StartTyping))]
    private void OnUserStartTyping(Intercept e)
    {
        if (IsActive) e.Block();
    }
}
