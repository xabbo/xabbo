using Xabbo.Messages;

namespace Xabbo.Models;

public struct HslU8(byte h, byte s, byte l) : IComposer
{
    /// <summary>
    /// The hue value.
    /// </summary>
    public byte H = h;
    /// <summary>
    /// The saturation value.
    /// </summary>
    public byte S = s;
    /// <summary>
    /// The luminance value.
    /// </summary>
    public byte L = l;

    public readonly void Compose(in PacketWriter p)
    {
        p.WriteInt(H);
        p.WriteInt(S);
        p.WriteInt(L);
    }
}
