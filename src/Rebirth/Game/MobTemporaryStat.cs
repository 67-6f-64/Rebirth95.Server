using System;
using Rebirth.Common.Types;
using Rebirth.Network;

namespace Rebirth.Game
{
	public class MobStatEntry
	{
		public int MobStatTypeID { get; set; }
		public short nOption { get; set; }
		public int rOption { get; set; }
		public short nSLV { get; set; }
		public DateTime tStartTime { get; set; }
		public short nDurationSeconds { get; set; }
		public int CharIdFrom { get; set; }
		public int nDelay { get; set; }

		public bool MovementAffectingStat
			=> MobStatTypeID == (int)MobStatType.Speed
			|| MobStatTypeID == (int)MobStatType.Stun
			|| MobStatTypeID == (int)MobStatType.Freeze
			|| MobStatTypeID == (int)MobStatType.Doom
			|| MobStatTypeID == (int)MobStatType.RiseByToss;

		public int Mask => 1 << (MobStatTypeID >> 32);
		public byte Set => (byte)(MobStatTypeID >> 5);

		public MobStatEntry(MobStatType nType, int nSkillID, short nOption, int liDurationSeconds)
		{
			MobStatTypeID = (int)nType;
			rOption = nSkillID;
			this.nOption = nOption;
			nDurationSeconds = (short)liDurationSeconds;
		}

		public void Encode(COutPacket p)
		{
			switch ((MobStatType)MobStatTypeID)
			{
				case MobStatType.Burned:
					p.Encode4(0); // size
								  //tCur = (int)v45;
								  //do
								  //{
								  //    v47 = ZList < MobStat::BURNED_INFO >::AddTail(&v4->lBurnedInfo);
								  //    v47->dwCharacterID = CInPacket::Decode4(v8);
								  //    v47->nSkillID = CInPacket::Decode4(v8);
								  //    v47->nDamage = CInPacket::Decode4(v8);
								  //    v47->tInterval = CInPacket::Decode4(v8);
								  //    v47->tEnd = CInPacket::Decode4(v8);
								  //    v48 = CInPacket::Decode4(v8);
								  //    v49 = tCur-- == 1;
								  //    v47->nDotCount = v48;
								  //    v47->tLastUpdate = v7;
								  //}
								  //while (!v49);
					break;
				case MobStatType.Disable:
					p.Encode1(false); // bInvincible
					p.Encode1(false); // bDisable
					break;
				default:
					p.Encode2(nOption);
					p.Encode4(rOption);
					p.Encode2(nDurationSeconds); // seconds
					break;
			}
		}
	}
}
