using Rebirth.Characters;
using Rebirth.Network;
using Rebirth.Server.Center;
using System.Collections.Generic;
using System.Linq;

namespace Rebirth.Game.Messenger
{
    public class CUIMessenger
    {
        public int UID { get; set; }

        private readonly Character[] Visitors = new Character[3];
        private readonly List<int> Invites = new List<int>();

        public CUIMessenger(Character c)
        {
            Visitors[0] = c;
        }

        public bool Contains(int dwCharId) => Visitors.FirstOrDefault(c => c.dwId == dwCharId) is object;

        public bool TryJoin(Character c)
        {
            if (Contains(c.dwId))
                return false;

            var nIdx = GetOpenVisitorSlot();
            if (Invites.Contains(c.dwId) && nIdx > 0)
            {
                c.SendPacket(SelfEnterResult((byte)nIdx));
                Broadcast(EnterResult(c, (byte)nIdx, true));

                for (int i = 0; i < 3; i++)
                {
                    if (Visitors[i] is null)
                        continue;

                    c.SendPacket(EnterResult(Visitors[i], (byte)i, false)); // maybe true??
                }

                Visitors[nIdx] = c;
                Invites.Remove(c.dwId);
                return true;
            }
            return false;
        }

        public void Leave(int dwCharId)
        {
            if (!Contains(dwCharId))
                return;

            var emptyslots = 0;
            for (var i = 0; i < 3; i++)
            {
                if (Visitors[i]?.dwId == dwCharId)
                {
                    Visitors[i] = null;
                    Broadcast(LeaveResult((byte)i));
                }

                if (Visitors[i] is null)
                    emptyslots += 1;
            }

            if (emptyslots >= 3)
                MasterManager.MessengerManager.Remove(this);
        }

        private int GetOpenVisitorSlot()
        {
            for (var i = 0; i < 3; i++)
            {
                if (Visitors[i] is null)
                    return i;
            }
            return -1;
        }

        public void Broadcast(COutPacket p, Character exclude = null)
        {
            using (p)
            {
                foreach (var c in Visitors)
                {
                    if (c?.dwId == exclude?.dwId)
                        continue;

                    c?.SendPacket(p);
                }
            }
        }

        public void UpdateAvatar(Character c)
        {
            for (var i = 0; i < 3; i++)
            {
                if (Visitors[i]?.dwId == c.dwId)
                {
                    Broadcast(UpdateAvatar((byte)i, c), c);
                    return;
                }
            }
        }

        public void TrySendInvite(Character pFrom, string sCharNameTo)
        {
            if (GetOpenVisitorSlot() < 0 || sCharNameTo.Length > Constants.MaxCharNameLength)
                return;

            var pTarget = MasterManager.CharacterPool.Get(sCharNameTo);

            pFrom.SendPacket(InviteResult(sCharNameTo, pTarget is object));

            if (pTarget is object)
            {
                if (!Invites.Contains(pTarget.dwId))
                {
                    pTarget.SendPacket(Invite(pFrom.Stats.sCharacterName, pFrom.ChannelNumber));
                }

                Invites.Add(pTarget.dwId);
            }
        }

        public void Blocked(int dwCharId, string sBlockedChar)
        {
            Broadcast(BlockedResult(sBlockedChar, false));
            Invites.Remove(dwCharId);
        }

        public void DoChat(Character pFrom, string sMsg)
        {
            if (sMsg.Length > Constants.MaxChatMessageLength || sMsg.Length <= 0)
            {
                pFrom.Socket.Disconnect();
                return;
            }

			CCurseProcess.ProcessString(sMsg, out string processedString);

            Broadcast(ChatMessage(processedString), pFrom);
        }

        private COutPacket EnterResult(Character c, byte nIdx, bool bNew)
        {
            var p = new COutPacket(SendOps.LP_Messenger);
            p.Encode1((byte)MSMPType.MSMP_Enter);
            p.Encode1(nIdx);
            c.GetLook().Encode(p);
            p.EncodeString(c.Stats.sCharacterName);
            p.Encode1(c.ChannelNumber);
            p.Encode1(bNew); // prolly always a 1
            return p;
        }

        private COutPacket SelfEnterResult(byte nIdx)
        {
            var p = new COutPacket(SendOps.LP_Messenger);
            p.Encode1((byte)MSMPType.MSMP_SelfEnterResult);
            p.Encode1(nIdx);
            return p;
        }

        private COutPacket LeaveResult(byte nIdx)
        {
            var p = new COutPacket(SendOps.LP_Messenger);
            p.Encode1((byte)MSMPType.MSMP_Leave);
            p.Encode1(nIdx);
            return p;
        }

        private COutPacket Invite(string sInviter, byte nChannel)
        {
            var p = new COutPacket(SendOps.LP_Messenger);
            p.Encode1((byte)MSMPType.MSMP_Invite);
            p.EncodeString(sInviter);
            p.Encode1(nChannel);
            p.Encode4(UID);
            p.Encode1(0); // this does something but idk
            return p;
        }

        private COutPacket InviteResult(string sCharName, bool bSuccess)
        {
            var p = new COutPacket(SendOps.LP_Messenger);
            p.Encode1((byte)MSMPType.MSMP_InviteResult);
            p.EncodeString(sCharName);
            p.Encode1(bSuccess);
            return p;
        }

        private COutPacket BlockedResult(string sBlockedUser, bool bNotAcceptingChat)
        {
            var p = new COutPacket(SendOps.LP_Messenger);
            p.Encode1((byte)MSMPType.MSMP_Blocked);
            p.EncodeString(sBlockedUser);
            p.Encode1(bNotAcceptingChat);

            // if bNotAcceptingChat
            // [IDX794] "'%s' is currently not accepting chat."
            // else
            // [IDX795] "'%s' denied the request."

            return p;
        }

        private COutPacket ChatMessage(string sMsg)
        {
            var p = new COutPacket(SendOps.LP_Messenger);
            p.Encode1((byte)MSMPType.MSMP_Chat);
            p.EncodeString(sMsg);
            return p;
        }

        private COutPacket UpdateAvatar(byte nIdx, Character c)
        {
            var p = new COutPacket(SendOps.LP_Messenger);
            p.Encode1((byte)MSMPType.MSMP_Avatar);
            p.Encode1(nIdx);
            c.GetLook().Encode(p);
            return p;
        }
    }
}
