using System;
using Autofac;
using Rebirth.Client;
using Rebirth.Commands;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;

namespace Rebirth.Characters.Actions
{
    public enum GroupMessageType
    {
        BuddyChat = 0,
        PartyChat = 1,
        ExpeditionChat = 2,
        GuildChat = 3,
        AllianceChat = 4,
    }

    public enum FindResult
    {
        ITC = 0,
        SameChannel = 1,
        CS = 2,
        DifferentChannel = 3,
    }

    public class ChatAction
    {
        public static void Handle_GroupMessage(WvsGameClient c, CInPacket p)
        {
            // : [8C 00] [5B BE 87 0F] [01] [01] [69 04 00 00] [03 00] [61 73 64]

            var get_update_time = p.Decode4();
            var nChatTarget = p.Decode1();
            var nMemberCnt = p.Decode1();
            var aMemberList = p.DecodeIntArray(nMemberCnt);
            var sText = p.DecodeString();

            // [INFO] Recv [CP_GroupMessage] 8C 00 30 9F 4F 0F 01 01 69 04 00 00 05 00 61 73 64 77 64

            if (sText.Length >= sbyte.MaxValue)
            {
                c.Character.SendMessage("Chat message exceeded allowed length.");
                return;
            }

            MasterManager.Log.Debug("Text: " + sText);

            switch ((GroupMessageType) nChatTarget)
            {
                case GroupMessageType.BuddyChat:
                    c.Character.Friends.HandleBuddyChat(sText, aMemberList);
                    break;
                case GroupMessageType.PartyChat:
                    c.Character.Party?.HandlePartyChat(c.Character.dwId, c.Character.Stats.sCharacterName, sText);
                    break;
                case GroupMessageType.ExpeditionChat:
                    // todo
                    break;
                case GroupMessageType.GuildChat:
                    // todo
                    break;
                case GroupMessageType.AllianceChat:
                    // todo
                    break;
            }
        }

        public static void Handle_CoupleMessage(WvsGameClient c, CInPacket p)
        {
            var sMarriedMsg = p.DecodeString();

            // todo 
        }

        public static void Handle_BroadcastMsg(WvsGameClient c, CInPacket p)
        {
            // TODO
        }

        public static void UserChat(WvsGameClient c, CInPacket p)
        {
            p.Decode4();
            var sText = p.DecodeString();
            var bOnlyBalloon = p.Decode1() != 0;

            if (sText.Length >= sbyte.MaxValue) return;

            var handle = ServerApp.Container.Resolve<CommandHandle>();

            if (!handle.Execute(c.Character, sText))
            {
                var bAdmin = c.Account.AccountData.Admin > 0;
                CCurseProcess.ProcessString(sText, out var output);

                if (output.Length <= 0) return;
                
                c.Character.Field.Broadcast(CPacket.UserChat(c.Character.dwId, output, bAdmin, bOnlyBalloon));
            }
        }

        public static void Handle_Whisper(WvsGameClient c, CInPacket p)
        {
            // Recv [CP_Whisper] [8D 00] [05] [7C 9E D7 14] [05 00] [70 65 6E 69 73]
            var flag = p.Decode1();  //    COutPacket::Encode1((COutPacket *)&oPacket, ((v44 == 0) + 1) | 4);
            p.Decode4(); // tick
            var name = p.DecodeString();

            if (name.Length > 13) return; 

            var pTargetUser = MasterManager.CharacterPool.Get(name, false);

            if (pTargetUser is null) // will not be null if char is in itc or cs
            {
                c.SendPacket(WhisperResult(name, false));
                return;
            }

            switch (flag) // todo make this an enumerator
            {
                case 5: // /find command
                    {
                        var nTargetChannel = pTargetUser.ChannelID;
                        if (MasterManager.CharacterPool.CharInCashShop(pTargetUser.dwId))
                        {
                            c.SendPacket(FindResult(pTargetUser.Stats.sCharacterName, Actions.FindResult.CS));
                        }
                        else if (nTargetChannel == c.ChannelId)
                        {
                            c.SendPacket(FindResult(pTargetUser.Stats.sCharacterName, Actions.FindResult.SameChannel, pTargetUser.Field.MapId));
                        }
                        else
                        {
                            c.SendPacket(FindResult(pTargetUser.Stats.sCharacterName, Actions.FindResult.DifferentChannel, pTargetUser.ChannelID));
                        }
                    }
                    break;
                case 6: // whisper
                    {
                        var msg = p.DecodeString();

                        if (msg.Length > 75) return; // PE or trying to find player

                        pTargetUser.SendPacket(WhisperMessage(c.Character.Stats.sCharacterName, c.Character.ChannelID, c.Character.Account?.AccountData.Admin > 0, msg));
                        c.SendPacket(WhisperResult(c.Character.Stats.sCharacterName, true));
                    }
                    break;
            }
        }

        /// <summary>
        /// Send to the person whispering
        /// </summary>
        /// <param name="sName">Sender name</param>
        /// <param name="bSuccess">If the target was found or not</param>
        /// <returns></returns>
        private static COutPacket WhisperResult(string sName, bool bSuccess)
        {
            var p = new COutPacket(SendOps.LP_Whisper);
            p.Encode1(0xA); // todo enum
            p.EncodeString(sName);
            p.Encode1(bSuccess);
            return p;
        }

        /// <summary>
        /// Send to whisper target
        /// </summary>
        /// <param name="sName">Sender name</param>
        /// <param name="nChannel"></param>
        /// <param name="bFromAdmin"></param>
        /// <param name="sMsg"></param>
        /// <returns></returns>
        private static COutPacket WhisperMessage(string sName, byte nChannel, bool bFromAdmin, string sMsg)
        {
            var p = new COutPacket(SendOps.LP_Whisper);
            p.Encode1(0x12); // todo enum
            p.EncodeString(sName);
            p.Encode1(nChannel);
            p.Encode1(bFromAdmin);
            p.EncodeString(sMsg);

            return p;
        }

        /// <summary>
        /// If char is in another channel, put channel id
        /// If char is in same channel, put map id
        /// </summary>
        private static COutPacket FindResult(string sName, FindResult nFindResult, int nFieldOrChannel = -1)
        {
            var p = new COutPacket(SendOps.LP_Whisper);
            p.Encode1(0x9); // todo enum
            p.EncodeString(sName);
            p.Encode1((byte)nFindResult);

            switch (nFindResult)
            {
                case Actions.FindResult.ITC:
                case Actions.FindResult.CS:
                    p.Encode4(-1);
                    break;
                case Actions.FindResult.DifferentChannel:

	                if (nFieldOrChannel > 20) nFieldOrChannel = 0;

                    p.Encode4(nFieldOrChannel);
                    break;
                case Actions.FindResult.SameChannel:
                    p.Encode4(nFieldOrChannel);
                    break;
            }

            if (nFindResult == Actions.FindResult.SameChannel)
            {
                p.Encode8(0); // idk
            }

            return p;
        }
    }
}
