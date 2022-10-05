using System;

using Microsoft.Extensions.Configuration;

using Xabbo.Extension;
using Xabbo.Messages;

namespace b7.Xabbo.Components;

public class AntiWalkComponent : Component
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

    [InterceptOut(nameof(Outgoing.Move)), RequiredOut(nameof(Outgoing.LookTo))]
    private void OnMove(InterceptArgs e)
    {
        if (IsActive) e.Block();

        if (FaceDirection)
        {
            int x = e.Packet.ReadInt();
            int y = e.Packet.ReadInt();
            Extension.Send(Out.LookTo, x, y);
        }
    }
}
