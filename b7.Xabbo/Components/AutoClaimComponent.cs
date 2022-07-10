using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components
{
    public class AutoClaimComponent : Component
    {
        private readonly XabbotComponent _xabbot;

        public AutoClaimComponent(
            IInterceptor interceptor,
            IConfiguration config,
            XabbotComponent xabbot)
            : base(interceptor)
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
                Send(Out.ClaimEarning, (byte)1);
                _xabbot.ShowMessage("Claimed daily reward.");
            }
        }
    }
}
