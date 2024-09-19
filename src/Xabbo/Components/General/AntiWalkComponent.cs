using Xabbo.Extension;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Components;

[Intercept]
public partial class AntiWalkComponent(IExtension extension) : Component(extension)
{
    [Reactive] bool Enabled { get; set; }

    private bool _faceDirection;
    public bool FaceDirection
    {
        get => _faceDirection;
        set => Set(ref _faceDirection, value);
    }

    [Intercept]
    private void OnMove(Intercept e, WalkMsg walk)
    {
        if (Enabled) e.Block();

        if (FaceDirection)
            Ext.Send(new LookToMsg(walk.Point));
    }
}
