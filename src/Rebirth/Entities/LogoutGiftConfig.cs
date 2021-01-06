using Rebirth.Network;

namespace Rebirth.Entities
{
    public class LogoutGiftConfig
    {
        public int bPredictQuit { get; set; }
        public int[] nLogoutGiftCommoditySN { get; }

        public LogoutGiftConfig()
        {
            bPredictQuit = 0;
            nLogoutGiftCommoditySN = new int[] { 2022728 , 2022728 , 2022728 }; // 1hr exp coupons
        }

        public void Encode(COutPacket p)
        {
            p.Encode4(bPredictQuit);

            foreach (var gift in nLogoutGiftCommoditySN)
                p.Encode4(gift);
        }

        public static COutPacket OnLogoutGift()
        {
            var p = new COutPacket(SendOps.LP_LogoutGift);
            return p;
        }
    }
}
