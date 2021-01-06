using Rebirth.Entities;
using Rebirth.Entities.PlayerData;
using Rebirth.Game;
using Rebirth.Network;

namespace Rebirth
{
	public partial class CPacket
	{
		public static class CLogin
		{
			public static COutPacket WorldInformation(byte nWorldID, string sName, byte nChannelNo, CLoginBalloon[] balloons)
			{
				var p = new COutPacket();

				//v6 = ZArray<CLogin::WORLDITEM>
				p.Encode2((short)SendOps.LP_WorldInformation);
				p.Encode1(nWorldID); //v4 [Server ID]  
				p.EncodeString(sName); //WORLDITEM->sName
				p.Encode1(1); //v6->nWorldState | 0 = normal 1 = hot? 2 = new
				p.EncodeString("Event Message?"); //sWorldEventDesc
				p.Encode2(100); //v6->nWorldEventEXP_WSE
				p.Encode2(100); //v6->nWorldEventDrop_WSE
				p.Encode1(0); //v6->nBlockCharCreation

				p.Encode1(nChannelNo); //v9

				for (byte i = 0; i < nChannelNo; i++)
				{
					//v11 =  ZArray<CLogin::CHANNELITEM>
					p.EncodeString($"{sName}-{i + 1}");
					p.Encode4(0); //v11->nUserNo
					p.Encode1(nWorldID); //v11->nWorldID
					p.Encode1(i); //v11->nChannelID
					p.Encode1(0); //v11->bAdultChannel
				}

				p.Encode2((short)balloons.Length);  //v2->m_nBalloonCount 

				foreach (var b in balloons)
					b.Encode(p);

				return p;
			}

			public static COutPacket WorldInformationEnd()
			{
				var p = new COutPacket(SendOps.LP_WorldInformation);
				p.Encode1(0xFF); //nWorldID
				return p;
			}

			public static COutPacket LatestConnectedWorld(byte nWorldID)
			{
				var p = new COutPacket(SendOps.LP_LatestConnectedWorld);
				p.Encode4(nWorldID);
				return p;
			}

			public static COutPacket SelectWorldResult(CharacterEntry[] chars, int charSlots)
			{
				var p = new COutPacket(SendOps.LP_SelectWorldResult);

				var charCount = (byte)chars.Length;

				p.Encode1(0); //some sort of error sresposne
				p.Encode1(charCount); //chars count

				foreach (var entry in chars)
				{
					entry.Encode(p);
				}

				p.Encode1(2); //m_bLoginOpt | spw request?

				p.Encode4(charSlots); //m_nSlotCount
				p.Encode4(0); //m_nBuyCharCount | https://i.imgur.com/DMynDxG.png

				return p;
			}

			public static COutPacket CheckUserLimit(byte status)
			{
				var p = new COutPacket(SendOps.LP_CheckUserLimitResult);

				/* 
                 * 0 - Normal
                 * 1 - Highly populated
                 * 2 - Full
                 */

				p.Encode1(0); //bOverUserLimit
				p.Encode1(status); //bPopulateLevel

				return p;
			}

			public static COutPacket ViewAllCharResult_Error(string szMessage)
			{
				var p = new COutPacket(SendOps.LP_ViewAllCharResult);
				p.Encode1(3);
				p.Encode1(1);
				p.EncodeString(szMessage);
				return p;

			}

			public static COutPacket DeleteCharacter(int uid, byte result)
			{
				var p = new COutPacket(SendOps.LP_DeleteCharacterResult);
				p.Encode4(uid);

				// 6 : Trouble logging in? Try logging in again from maplestory.nexon.net.
				// 9 : Failed due to unknown reason.
				// 10 : Could not be processed due to too many connection requests to the server. Please try again later.
				// 18 : The 8-digit birthday code you have entered is incorrect.
				// 20 : You have entered an incorrect PIC.
				// 22 : Cannot delete Guild Master character.
				// 24 : You may not delete a character that has been engaged or booked for a wedding.
				// 26 : You cannot delete a character that is currently going through the transfer.
				// 29 : You may not delete a character that has a family.

				p.Encode1(result);
				return p;
			}

			public static COutPacket SelectCharacterResult(int uid, byte[] ip, short port)
			{
				var p = new COutPacket(SendOps.LP_SelectCharacterResult);

				p.Encode1(0); //v3 | World
				p.Encode1(0); //dwCharacterID | Selectec Char

				p.EncodeBuffer(ip, 0, ip.Length);
				p.Encode2(port);
				p.Encode4(uid);
				p.Encode1(0);
				p.Encode4(0);

				//v8 = CInPacket::Decode4(iPacket);
				//v9 = CInPacket::Decode2(iPacket);
				//v10 = CInPacket::Decode4(iPacket);
				//bAuthenCode = CInPacket::Decode1(iPacket);
				//v12 = CInPacket::Decode4(iPacket);
				//ZInetAddr::ZInetAddr((ZInetAddr*)&addrNet, v9);

				return p;
			}

			public static COutPacket CheckPasswordResult(int accId, byte gender, byte nGradeCode, short nSubGradeCode, string accountName)
			{
				var p = new COutPacket(SendOps.LP_CheckPasswordResult);

				p.Encode1(0); //nRet
				p.Encode1(0); //nRegStatID
				p.Encode4(0); //nUseDay

				//if (nRet == 2)
				//{
				//    COutPacket::Encode1((COutPacket*)&oPacket, v2->m_nBlockReason);
				//    COutPacket::EncodeBuffer((COutPacket*)&oPacket, &v2->m_dtUnblockDate, 8u);
				//}

				p.Encode4(accId); // dwAccountId
				p.Encode1(gender); // nGender
				p.Encode1(nGradeCode); // nGradeCode
				p.Encode2(nSubGradeCode); // nSubGradeCode
				p.Encode1(0); // nCountryID
				p.EncodeString(accountName); // sNexonClubID
				p.Encode1(0); // nPurchaseExp
				p.Encode1(0); // nChatBlockReason
				p.Encode8(0); // dtChatUnblockDate
				p.Encode8(0); // dtRegisterDate
				p.Encode4(4); // flag that affects character creation

				p.Encode1(true); //If true client will send CP_WorldRequest
								 //If false it check nGameStartMode and returns or sends CP_CheckPinCode

				p.Encode1(1); //sMsg._m_pStr[432] = CInPacket::Decode1(iPacket);
				p.Encode8(0); // dwHighDateTime
				return p;
			}

			public static COutPacket CheckPasswordResult(int reason)
			{
				// 3: ID deleted or blocked
				// 4: Incorrect password
				// 5: Not a registered id
				// 6: System error
				// 7: Already logged in
				// 8: System error
				// 9: System error
				// 10: Cannot process so many connections
				// 11: Only users older than 20 can use this channel
				// 13: Unable to log on as master at this ip
				// 14: Wrong gateway or personal info and weird korean button
				// 15: Processing request with that korean button!
				// 16: Please verify your account through email...
				// 17: Wrong gateway or personal info
				// 21: Please verify your account through email...
				// 23: License agreement
				// 25: Maple Europe notice
				// 27: Some weird full client notice, probably for trial versions
				// 32: IP blocked
				// 84: please revisit website for pass change --> 0x07 recv with response 00/01

				var p = new COutPacket(SendOps.LP_CheckPasswordResult);
				p.Encode4(reason);
				p.Encode2(0);
				return p;
			}
		}
	}
}