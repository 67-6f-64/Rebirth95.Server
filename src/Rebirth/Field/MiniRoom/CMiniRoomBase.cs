using Rebirth.Characters;
using Rebirth.Client;
using Rebirth.Network;
using Rebirth.Server.Center;
using System.Collections.Generic;
using static Rebirth.Field.MiniRoom.MiniRoomEnum;

namespace Rebirth.Field.MiniRoom
{
    public abstract class CMiniRoomBase : CFieldObj
    {
        public readonly LinkedList<KeyValuePair<string, byte>> aChatText = new LinkedList<KeyValuePair<string, byte>>();
        public byte nMaxUsers { get; set; }
        public byte nCurUsers { get; set; }
        public bool bTournament { get; set; }

        public MR_Type nMiniRoomType { get; }

        public int OwnerID { get; }
        public Character Parent => MasterManager.CharacterPool.Get(OwnerID);
        public string OwnerName => Parent.Stats.sCharacterName;
        public bool Destroyed { get; set; }

        // --------------------------------------------------- constructor

        public CMiniRoomBase(Character owner, MR_Type type)
        {
            Position = owner.Position.Clone();
            nMiniRoomType = type;
            OwnerID = owner.dwId;
        }

        // --------------------------------------------------- abstracts

        public abstract void HandleChat(Character from, string message);
        public abstract void Refresh(Character c);
        public abstract void Create();
        public abstract bool Expired();

        public virtual void Destroy()
        {
            Destroyed = true;

            Field.MiniRooms.Remove(this);
        }
        public virtual void HandlePlayerExit(Character c, MR_LeaveResult nResult)
        {
            nCurUsers -= 1;
            c.CurMiniRoom = null;
        }
        public virtual void HandleVisit(CInPacket p, WvsGameClient c)
        {
            nCurUsers += 1;
            c.Character.CurMiniRoom = this;
        }

        // -------------------------------------------- inherited packets

        public override COutPacket MakeEnterFieldPacket() => CMiniRoomPool.EmployeeEnterField(this); // only used for player shops / hired merchants
        public override COutPacket MakeLeaveFieldPacket() => CMiniRoomPool.EmployeeLeaveField(dwId);

        public void EncodeAvatar(COutPacket p, Character pChar, byte index)
        {
            p.Encode1(index);
            pChar.GetLook().Encode(p);
            p.EncodeString(pChar.Stats.sCharacterName);
            p.Encode2(pChar.Stats.nJob);
        }
    }
}