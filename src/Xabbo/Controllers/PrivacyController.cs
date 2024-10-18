using Xabbo.Configuration;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Extension;
using Xabbo.Services.Abstractions;

namespace Xabbo.Controllers;

[Intercept]
public sealed partial class PrivacyController(
    IExtension extension,
    IConfigProvider<AppConfig> config,
    ProfileManager profileManager
)
    : ControllerBase(extension)
{
    private readonly IConfigProvider<AppConfig> _config = config;
    private readonly ProfileManager _profileManager = profileManager;

    [Intercept]
    void HandleAvatars(Intercept e, AvatarsAddedMsg msg)
    {
        if (!_config.Value.General.PrivacyMode)
            return;

        foreach (var avatar in msg)
        {
            if (avatar is User user)
            {
                if (user.Name != _profileManager.UserData?.Name)
                    user.Name = new string('_', user.Name.Length);
            }
        }

        e.Packet.Clear();
        e.Packet.Write(msg);
    }
}