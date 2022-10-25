using Microsoft.Extensions.Configuration;

using Xabbo.Extension;
using Xabbo.Messages;

namespace b7.Xabbo.Components;

public class AutoClaimComponent : Component
{
    private readonly XabbotComponent _xabbot;

    public AutoClaimComponent(
        IExtension extension,
        IConfiguration config,
        XabbotComponent xabbot)
        : base(extension)
    {
        _xabbot = xabbot;

        IsActive = config.GetValue("AutoClaimDailyReward:Active", false);
    }

    [InterceptIn(nameof(Incoming.EarningStatus))]
    protected void OnEarningStatus(InterceptArgs e)
    {
        if (!IsActive) return;

        bool claimReward = false;

        int n = e.Packet.ReadInt();
        for (int i = 0; i < n; i++)
        {
            int rewardCategory = e.Packet.ReadByte();
            int rewardType = e.Packet.ReadByte();
            int amount = e.Packet.ReadInt();
            string productCode = e.Packet.ReadString();

            if (rewardCategory == 1 &&
                amount > 0)
            {
                claimReward = true;
            }
        }

        if (claimReward)
        {
            Extension.Send(Out.ClaimEarning, (byte)1);
            _xabbot.ShowMessage("Claimed daily reward.");
        }
    }
}
