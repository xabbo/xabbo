using Microsoft.Extensions.Configuration;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Core.Messages.Outgoing;

namespace b7.Xabbo.Components;

[Intercept]
public partial class AntiWalkComponent : Component
{
    private bool _faceDirection;
    public bool FaceDirection
    {
        get => _faceDirection;
        set => Set(ref _faceDirection, value);
    }

    public AntiWalkComponent(IExtension extension,
        IConfiguration config)
        : base(extension)
    {
        IsActive = config.GetValue("AntiWalk:Active", false);
        FaceDirection = config.GetValue("AntiWalk:FaceDirection", false);
    }

    [Intercept]
    // [RequiredOut(nameof(Out.LookTo))]
    private void OnMove(Intercept e, WalkMsg walk)
    {
        if (IsActive) e.Block();

        if (FaceDirection)
            Ext.Send(new LookToMsg(walk.X, walk.Y));
    }
}
