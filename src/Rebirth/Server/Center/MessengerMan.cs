using Rebirth.Client;
using Rebirth.Game.Messenger;
using Rebirth.Network;
using System.Linq;
using Rebirth.Tools;

namespace Rebirth.Server.Center
{
    public class MessengerMan : NumericKeyedCollection<CUIMessenger>
    {
        private readonly LoopingID MessengerUID = new LoopingID();

        public MessengerMan() { }

        public void OnPacket(WvsGameClient c, CInPacket p)
        {
            var MSMP = p.Decode1();

            switch ((MSMPType)MSMP)
            {
                case MSMPType.MSMP_Enter:
                    var id = p.Decode4();

                    if (Contains(id))
                    {
                        if (!this[id].TryJoin(c.Character))
                        {
                            c.Character.SendMessage("The room is full.");
                        }
                    }
                    else
                    {
                        Add(new CUIMessenger(c.Character));
                    }

                    break;
                case MSMPType.MSMP_SelfEnterResult:
                    break;
                case MSMPType.MSMP_Leave:
                    GetByCharId(c.Character.dwId)?.Leave(c.Character.dwId);
                    break;
                case MSMPType.MSMP_Invite: // same in/out
                    GetByCharId(c.Character.dwId)?.TrySendInvite(c.Character, p.DecodeString());
                    break;
                case MSMPType.MSMP_InviteResult:
                    break;
                case MSMPType.MSMP_Blocked:
                    var sInviter = p.DecodeString();

                    if (sInviter.Length > Constants.MaxCharNameLength)
                        return;

                    var pInviter = MasterManager.CharacterPool.Get(sInviter);

                    if (pInviter is null)
                        return;

                    GetByCharId(pInviter.dwId)?.Blocked(c.Character.dwId, c.Character.Stats.sCharacterName);
                    break;
                case MSMPType.MSMP_Chat:
                    GetByCharId(c.Character.dwId)?.DoChat(c.Character, p.DecodeString());
                    break;
                case MSMPType.MSMP_Avatar:
                    break;
                case MSMPType.MSMP_Migrated:
                    break;
            }
        }

        public CUIMessenger GetByCharId(int dwCharId) => this.FirstOrDefault(m => m.Contains(dwCharId));

        protected override void InsertItem(int index, CUIMessenger item)
        {
            item.UID = MessengerUID.NextValue();
            base.InsertItem(index, item);
        }

        protected override int GetKeyForItem(CUIMessenger item) => item.UID;
    }
}
