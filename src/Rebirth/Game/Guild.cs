using Npgsql;
using Rebirth.Characters;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebirth.Tools;

namespace Rebirth.Game
{
	public sealed class Guild : NumericKeyedCollection<GuildMember>
	{
		public GuildManager Manager { get; private set; }

		public int AllianceID { get; set; }
		public int GuildID { get; set; }

		public string AllianceName { get; set; } = "";
		public string GuildName { get; set; }

		public int LeaderID { get; set; }
		public short GuildLevel { get; set; } // TODO make this load on guild creation based on total guild points
		public int GuildPoints { get; set; }

		public short GuildMark { get; set; }
		public short GuildMarkBg { get; set; }
		public byte GuildMarkColor { get; set; }
		public byte GuildMarkBgColor { get; set; }

		public short Capacity { get; set; }

		public string Notice { get; set; }
		public string Rankt1Title { get; set; }
		public string Rankt2Title { get; set; }
		public string Rankt3Title { get; set; }
		public string Rankt4Title { get; set; }
		public string Rankt5Title { get; set; }

		public readonly List<int> Invites;

		public Guild(GuildManager gm)
		{
			Manager = gm;
			Invites = new List<int>();
		}

		public void Broadcast(COutPacket p, int dwCharExcluded = 0)
		{
			using (p)
			{
				foreach (var member in this)
				{
					if (member.dwParentID == dwCharExcluded) continue;

					MasterManager.CharacterPool
						.Get(member.dwParentID, false)
						?.SendPacket(p);
				}
			}
		}

		public void EncodeGuildData(COutPacket p) // GUILDDATA::Decode(&v2->m_guild, iPacket);
		{
			p.Encode4(GuildID); // this->nGuildID = CInPacket::Decode4(iPacket);
			p.EncodeString(GuildName); // v4 = CInPacket::DecodeStr(v3, &iPacket);

			// asGradeName ZArray<ZXString<char> >
			p.EncodeString(Rankt1Title);
			p.EncodeString(Rankt2Title);
			p.EncodeString(Rankt3Title);
			p.EncodeString(Rankt4Title);
			p.EncodeString(Rankt5Title);

			// adwCharacterID ZArray<unsigned long>
			p.Encode1((byte)Count); // v7 = CInPacket::Decode1(v3);

			//var orderedMembers = this.OrderBy(member => member.GuildRank);

			foreach (var member in this) // CInPacket::DecodeBuffer(v3, v2->adwCharacterID.a, 4 * v7);
			{
				p.Encode4(member.dwParentID);
			}

			foreach (var member in this) // CInPacket::DecodeBuffer(v3, v2->aMemberData.a, 37 * v7);
			{
				member.EncodeGuildMember(p);
			}

			p.Encode4(Capacity); // nMaxMemberNum dd
			p.Encode2(GuildMarkBg); // nMarkBg dw
			p.Encode1(GuildMarkBgColor); // nMarkBgColor db
			p.Encode2(GuildMark); // nMark dw
			p.Encode1(GuildMarkColor); // nMarkColor db
			p.EncodeString(Notice); // sNotice ZXString<char>

			p.Encode4(GuildPoints); // nPoint dd
			p.Encode4(AllianceID); // nAllianceID dd
			p.Encode1((byte)GuildLevel); // nLevel dd

			p.Encode2(0); // mSkillRecord ZMap<long,GUILDDATA::SKILLENTRY,long>
		}

		protected override void InsertItem(int index, GuildMember item)
		{
			if (item != null)
			{
				item.Guild = this;
				Manager.AddCharIDToCache(item.dwParentID, GuildID);
				base.InsertItem(index, item);
			}
		}

		protected override void RemoveItem(int index)
		{
			base.RemoveItem(index);
		}

		protected override void ClearItems()
		{
			// order of operations is important
			this.ForEach(member => Manager.RemoveCharIDFromCache(member.dwParentID));
			Manager = null;

			base.ClearItems();
		}

		protected override int GetKeyForItem(GuildMember item) => item.dwParentID;
	}
}