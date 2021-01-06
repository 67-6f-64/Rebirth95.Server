using Rebirth.Characters;
using Rebirth.Client;
using Rebirth.Network;
using System;
using static Rebirth.Field.MiniRoom.MiniRoomEnum;

namespace Rebirth.Field.MiniRoom
{
    public sealed class CPersonalShop : CMiniRoomBase
    {
        public int nTemplateId { get; set; }
        public string sTitle { get; set; }

        public CPersonalShop(Character owner, int templateId, string title) : base(owner, MR_Type.PersonalShop)
        {
            nTemplateId = templateId;
            sTitle = title;
        }

        public override void HandleVisit(CInPacket p, WvsGameClient c)
        {
            throw new NotImplementedException();
        }

        public override void HandlePlayerExit(Character c, MR_LeaveResult nReason)
        {
            throw new NotImplementedException();
        }

        public override void HandleChat(Character from, string message)
        {
            throw new NotImplementedException();
        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public override void Refresh(Character c)
        {
            throw new NotImplementedException();
        }

        public override void Create()
        {
            throw new NotImplementedException();
        }

        public override bool Expired()
        {
            throw new NotImplementedException();
        }
    }
}
