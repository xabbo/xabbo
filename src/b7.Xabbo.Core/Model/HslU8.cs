using Xabbo.Messages;

namespace b7.Xabbo.Model;

public struct HslU8(byte h, byte s, byte l) : IComposable
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

    public readonly void Compose(IPacket packet)
    {
        packet.WriteInt(H).WriteInt(S).WriteInt(L);
    }
}
