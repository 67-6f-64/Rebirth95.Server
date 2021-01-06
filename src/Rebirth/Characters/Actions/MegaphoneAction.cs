using Rebirth.Entities.Item;
using Rebirth.Network;
using System;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Actions
{
    /// <summary>
    /// TODO find a better spot for these
    /// </summary>
    public sealed class MegaphoneAction
    {
        public static COutPacket AvatarMegaphonePacket(bool bWhisperIcon, byte nChannelNo, params string[] sMsg)
        {
            throw new NotImplementedException();
        }

        public static COutPacket MegaphonePacket(CashItemType nType, bool bWhisperIcon, byte nChannelNo, params string[] sMsg)
        {
            var p = new COutPacket(SendOps.LP_BroadcastMsg);
            switch (nType) // idk if i like this implementation....
            {
                case CashItemType.SPEAKERCHANNEL:
					EncodeMegaphoneText(p, BroadcastMsgType.SpeakerChannel, sMsg[0]);
                    break;
                case CashItemType.SPEAKERWORLD:
					EncodeMegaphoneText(p, BroadcastMsgType.SpeakerWorld, sMsg[0]);
					break;
                case CashItemType.SKULLSPEAKER:
					EncodeMegaphoneText(p, BroadcastMsgType.SkullSpeaker, sMsg[0]);
					break;
                case CashItemType.ARTSPEAKERWORLD:
                    p.Encode1((byte)BroadcastMsgType.ArtSpeakerWorld);
					p.EncodeString(sMsg[0]);
					p.Encode1((byte)sMsg.Length);
					for (int i = 1; i < sMsg.Length; i++)
					{
						p.EncodeString(sMsg[i]);
					}
                    break;
            }

            p.Encode1(nChannelNo);
            p.Encode1(bWhisperIcon);

            return p;
        }

		private static void EncodeMegaphoneText(COutPacket p, BroadcastMsgType nType, string sMsg)
		{
			p.Encode1((byte)nType);
			p.EncodeString(sMsg);
		}

        public static COutPacket ItemMegaphonePacket(string sMsg, byte nChannelNo, bool bWhisperIcon, GW_ItemSlotBase pItem)
        {
            var p = new COutPacket(SendOps.LP_BroadcastMsg);
            p.Encode1((byte)(pItem is null 
                ? BroadcastMsgType.SpeakerBridge 
                : BroadcastMsgType.ItemMegaphone));
            p.EncodeString(sMsg);
            p.Encode1(nChannelNo);
            p.Encode1(bWhisperIcon);
            p.Encode1(pItem is object);
            pItem?.RawEncode(p);
            return p;
        }
    }
}
