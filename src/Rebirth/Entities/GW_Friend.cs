using Rebirth.Characters;
using Rebirth.Network;
using Rebirth.Server.Center;

namespace Rebirth.Entities
{
	public sealed class GW_Friend
	{
		public Character Friend => MasterManager.CharacterPool.Get(dwFriendID, true);

		public int dwFriendID { get; set; }
		public string sFriendName { get; set; }
		public int nFlag { get; set; }
		public int nChannelID { get; set; }
		public string sFriendGroup { get; set; }

		public bool bCashShop { get; set; }
		public bool bOnline { get; set; }

		public void Encode(COutPacket p)
		{
			p.Encode4(dwFriendID);
			p.EncodeStringFixed(sFriendName, 13);
			p.Encode1((byte)(nFlag == 10 ? 0 : nFlag));
			p.Encode4(bOnline && !bCashShop ? nChannelID : -1);
			p.EncodeStringFixed(sFriendGroup, 17);
		}

		/// <summary>
		/// Sends a packet to the friend. Will not send a packet if friend is offline or in cash shop.
		/// </summary>
		/// <param name="p"></param>
		public void SendPacket(COutPacket p) => MasterManager.CharacterPool.Get(dwFriendID, false)?.SendPacket(p);
	}
}
