using log4net;
using Rebirth.Characters;
using Rebirth.Characters.Inventory;
using Rebirth.Characters.Modify;
using Rebirth.Client;
using Rebirth.Entities.Item;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using Rebirth.Common.Types;
using static Rebirth.Field.MiniRoom.MiniRoomEnum;

namespace Rebirth.Field.MiniRoom
{
    public class CTradingRoom : CMiniRoomBase
    {
        // ----------------------------------------------------- variables

        public static ILog Log = LogManager.GetLogger(typeof(CTradingRoom));

        private Character Partner;
        private readonly DateTime expiration;
        private int dwFirstLockerId;
        private MR_LeaveResult CurrentStatus;
        private bool acceptedInvite;

        private TempInventory tempInvOwner, tempInvPartner; // assigned when request is accepted

        // ----------------------------------------------------- constructor / destructor

        public CTradingRoom(Character owner)
            : base(owner, MR_Type.TradingRoom)
        {
            nCurUsers = 1;
            nMaxUsers = 2;
            owner.CurMiniRoom = this;
            expiration = DateTime.Now.AddMinutes(30);
            CurrentStatus = MR_LeaveResult.UserRequest;
        }

        public override void Create()
        {

        }

        /// <summary>
        /// This is called when a player leaves the room
        /// </summary>
        /// <param name="c"></param>
        /// <param name="nReason"></param>
        public override void HandlePlayerExit(Character c, MR_LeaveResult nReason)
        {
            if (c.dwId != Parent.dwId && c.dwId != Partner?.dwId)
                return; // PE

            if (acceptedInvite)
            {
                CurrentStatus = MR_LeaveResult.Closed;
            }

            Destroy();
        }

        public override void Destroy()
        {
            base.Destroy();

            ProcessPlayerLeave(Parent);

            if (acceptedInvite)
                ProcessPlayerLeave(Partner);
        }

        private void ProcessPlayerLeave(Character c)
        {
            base.HandlePlayerExit(c, CurrentStatus);

            if (acceptedInvite)
            {
                var success = CurrentStatus == MR_LeaveResult.TradeDone;
                TransferItems(c, GetTempInv(c, success), success);
            }
            else if (Partner != null)
            {
                base.HandlePlayerExit(Partner, CurrentStatus);
            }

            c.SendPacket(LeavePacket(!IsOwner(c)));
        }

        // ----------------------------------------------------- handlers

        public override void HandleVisit(CInPacket p, WvsGameClient c) // accept invite
        {
            base.HandleVisit(p, c);

            if (c?.Character.dwId != Partner?.dwId)
                return;

            tempInvOwner = new TempInventory(nMiniRoomType, 9, OwnerID);
            tempInvPartner = new TempInventory(nMiniRoomType, 9, Partner.dwId);

            Parent.SendPacket(AcceptInvitePacket());
            Partner.SendPacket(GetMiniRoomPacket());

            acceptedInvite = true;
        }

        public void HandleSendInvite(int dwTargetCharId)
        {
            if (Partner != null || dwTargetCharId == OwnerID)
                return;

            var pTargetChar = MasterManager.CharacterPool.Get(dwTargetCharId);

            MR_InviteResult result;

            if (pTargetChar?.Field.MapId != Parent.Field.MapId)
            {
                result = MR_InviteResult.NoCharacter; // unable to find character
            }
            else if (pTargetChar.CurMiniRoom != null)
            {
                result = MR_InviteResult.CannotInvite; // doing something else right now
            }
            else
            {
                Partner = pTargetChar;
                Partner.CurMiniRoom = this; // required cuz the decline trade packet doesnt have room ID...
                Parent.SendPacket(GetMiniRoomPacket());
                Partner.SendPacket(InvitePacket(Parent.Stats.sCharacterName, dwId));
                return;
            }

            Parent.SendPacket(InviteResult(result, pTargetChar?.Stats.sCharacterName));
            Destroy();
        }

        public void HandleConfirmTrade(Character confirmer)
        {
            if (!acceptedInvite || confirmer.dwId == dwFirstLockerId)
                return; // PE

            if (IsOwner(confirmer))
                Partner.SendPacket(PartnerLockPacket());
            else
                Parent.SendPacket(PartnerLockPacket());

            if (dwFirstLockerId > 0)
            {
                if (InventoryManipulator.HasSpace(Parent, tempInvPartner) && InventoryManipulator.HasSpace(Parent, tempInvOwner))
                {
                    CurrentStatus = MR_LeaveResult.TradeDone;
                }
                else
                {
                    CurrentStatus = MR_LeaveResult.TradeFail;
                }
                Destroy();
            }
            else
            {
                dwFirstLockerId = confirmer.dwId;
            }
        }

        public void HandleSetMeso(Character pChar, int nAmount)
        {
            if (dwFirstLockerId == pChar.dwId || nAmount < 0 || !acceptedInvite)
                return;

            var tempInv = GetTempInv(pChar);

            if (pChar.Stats.nMoney < nAmount || nAmount > int.MaxValue - tempInv.Meso)
                return;

            pChar.Modify.GainMeso(-nAmount);

            tempInv.Meso += nAmount;

            Parent.SendPacket(SetMesoPacket((int)tempInv.Meso, !IsOwner(pChar)));
            Partner.SendPacket(SetMesoPacket((int)tempInv.Meso, IsOwner(pChar)));
        }

        public void HandleAddItem(CInPacket p, Character pChar)
        {
            if (dwFirstLockerId == pChar.dwId || !acceptedInvite)
                return;

            var nTI = (InventoryType)p.Decode1();
            var nCurInvSlot = p.Decode2(); // in inventory
            var nCount = p.Decode2();
            var nTargetSlot = p.Decode1(); // in trade window

            var pItem = InventoryManipulator.GetItem(pChar, nTI, nCurInvSlot);

            var tempInv = GetTempInv(pChar);

            if (pItem is null || !tempInv.CanAddToSlot(nTargetSlot))
                return;
			
            if (ItemConstants.is_treat_singly(pItem.nItemID))
            {
                nCount = -1; // negative amount clears the slot
            }
            else
            {
                nCount = Math.Min(pItem.nNumber, Math.Abs(nCount));
            }

            InventoryManipulator.RemoveFrom(pChar, nTI, nCurInvSlot, nCount);
            
            if (pItem is GW_ItemSlotBundle pBundle && !pBundle.IsRechargeable)
            {
	            pItem = pItem.DeepCopy();
	            pItem.nNumber = nCount;
            }

            var pTempItem = new TempItem(pItem, nCurInvSlot, nTargetSlot);

            tempInv.Add(pTempItem);

            Parent.SendPacket(AddItemPacket(pTempItem, !IsOwner(pChar)));
            Partner.SendPacket(AddItemPacket(pTempItem, IsOwner(pChar)));
        }

        public void HandleDecline()
        {
            Parent.SendPacket(InviteResult(MR_InviteResult.Rejected, Partner.Stats.sCharacterName));
            Partner.CurMiniRoom = null;
            Partner = null;
        }

        public override void HandleChat(Character from, string message)
        {
            Parent?.SendPacket(ChatPacket(from.Stats.sCharacterName, message, !IsOwner(from)));
            Partner?.SendPacket(ChatPacket(from.Stats.sCharacterName, message, IsOwner(from)));
        }

        public override void Refresh(Character c)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Transfer items from a temp inventory to a character inventory.
        /// No validation is done to see if the items will fit.
        /// At the end of this method the temp inventory is cleared.
        /// </summary>
        /// <param name="pChar">Character to transfer items to.</param>
        /// <param name="pTempInv">Inventory to transfer items from.</param>
        /// <param name="bSubtractFee">If the money being transferred should have the trade fee deducted.</param>
        private void TransferItems(Character pChar, TempInventory pTempInv, bool bSubtractFee)
        {
            foreach (var item in pTempInv.GetAll())
            {
                InventoryManipulator.InsertInto(pChar, item.Item);
            }

            if (pTempInv.Meso > 0)
            {
                if (bSubtractFee)
                {
                    pTempInv.Meso -= Fee((int)pTempInv.Meso);
                }

                pChar?.Modify.GainMeso((int)pTempInv.Meso);
            }

            pTempInv.Clear();
        }

        public override bool Expired()
            => expiration >= DateTime.Now;

        private bool IsOwner(Character c)
            => c.dwId == OwnerID;

        private TempInventory GetTempInv(Character c, bool bPartner = false)
            => IsOwner(c)
            ? (bPartner ? tempInvPartner : tempInvOwner)
            : (bPartner ? tempInvOwner : tempInvPartner);

        private int Fee(int meso) // todo refine this method
        {
            var fee = 0.0;
            if (meso >= 100000000)
            {
                fee = 0.06 * meso;
            }
            else if (meso >= 25000000)
            {
                fee = meso / 20.0;
            }
            else if (meso >= 10000000)
            {
                fee = meso / 25.0;
            }
            else if (meso >= 5000000)
            {
                fee = 0.03 * meso;
            }
            else if (meso >= 1000000)
            {
                fee = 0.018 * meso;
            }
            else if (meso >= 100000)
            {
                fee = meso / 125.0;
            }
            return (int)fee;
        }

        // ----------------------------------------------------- packets

        private COutPacket InviteResult(MR_InviteResult nType, string sTargetName)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);
            p.Encode1((byte)MR_Action.MRP_InviteResult);
            p.Encode1((byte)nType);
            if (sTargetName != null)
                p.EncodeString(sTargetName);
            return p;
        }

        private COutPacket AcceptInvitePacket()
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.MRP_Enter); // accept op code
            EncodeAvatar(p, Partner, 1);

            return p;
        }

        private COutPacket LeavePacket(bool bOwner)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.MRP_Leave); // exit op code
            p.Encode1(bOwner); // who exited
            p.Encode1((byte)CurrentStatus);

            return p;
        }

        public COutPacket GetMiniRoomPacket()
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.MRP_EnterResult); // room start op code
            p.Encode1((byte)nMiniRoomType); // room type
            p.Encode1(nMaxUsers); // 2
            p.Encode1((byte)(nCurUsers - 1));
            if (nCurUsers == 2)
            {
                EncodeAvatar(p, Parent, 0);
                EncodeAvatar(p, Partner, 1);
            }
            else
            {
                EncodeAvatar(p, Parent, 0);
            }
            p.Encode1(0xFF);

            return p;
        }

        private COutPacket InvitePacket(string name, int roomId)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.MRP_Invite); // trade invite opcode
            p.Encode1((byte)nMiniRoomType);
            p.EncodeString(name);
            p.Encode4(roomId); // this gets sent back in the response so we know how to track which one they are replying to

            return p;
        }

        private COutPacket ChatPacket(string name, string message, bool owner)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.MRP_Chat); // chat op code
            p.Encode1((byte)MR_Action.MRP_UserChat); // another op code related to chat
            p.Encode1(owner);
            p.EncodeString($"{name} : {message}");

            return p;
        }

        private COutPacket MesoLimitPacket()
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1(4); // meso limit opcode

            return p;
        }

        private COutPacket SetMesoPacket(int amount, bool owner)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.TRP_PutMoney); // set meso opcode
            p.Encode1(owner);
            p.Encode4(amount);

            return p;
        }

        private COutPacket AddItemPacket(TempItem item, bool owner)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.TRP_PutItem); // add item opcode
            p.Encode1(owner);
            p.Encode1((byte)item.TargetSlot);
            item.Item.RawEncode(p);

            return p;
        }

        private COutPacket PartnerLockPacket()
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.TRP_Trade); // confirm opcode

            return p;
        }
    }
}
