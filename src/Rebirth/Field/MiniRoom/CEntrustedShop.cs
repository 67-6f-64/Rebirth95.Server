using log4net;
using Rebirth.Characters;
using Rebirth.Characters.Inventory;
using Rebirth.Characters.Modify;
using Rebirth.Client;
using Rebirth.Entities.Item;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Common.Types;
using static Rebirth.Field.MiniRoom.MiniRoomEnum;

namespace Rebirth.Field.MiniRoom
{
	public class CEntrustedShop : CMiniRoomBase
    {
        public static ILog Log = LogManager.GetLogger(typeof(CEntrustedShop));

        public int nTemplateId { get; }
        public byte nItemMaxCount => (byte)MasterManager.ItemTemplate(nTemplateId).SlotMax;
        public int ItemTotalSold { get; private set; } = 0;

        public readonly TempInventory Inventory;

        public bool FirstTime { get; set; } = true;
        public bool ShopOpen { get; set; } = false;

        public string sEmployerName { get; set; }
        public string sTitle { get; set; }
        public int nMoney { get; set; }

        public readonly List<string> pBlackList = new List<string>();
        public readonly int[] aCurVisitors;
        public readonly LinkedList<KeyValuePair<string, int>> pVisitList = new LinkedList<KeyValuePair<string, int>>();
        public readonly List<SoldItemData> ItemsSold = new List<SoldItemData>();

        // --------------------------------------------------- constructor / destructor

        public CEntrustedShop(Character owner, int templateId, string title) : base(owner, MR_Type.EntrustedShop)
        {
            nTemplateId = templateId;
            Inventory = new TempInventory(nMiniRoomType, nItemMaxCount, owner.dwId);
            sTitle = title;
            nCurUsers = 1;
            nMaxUsers = 4;
            aCurVisitors = new int[nMaxUsers];
        }

        public bool HasItems() => Inventory.Count > 0 || Inventory.Meso > 0;

        public override void Create()
        {
            Field.Broadcast(MakeEnterFieldPacket());

            VisitorKickAll(MR_LeaveResult.StartManage);
            ShopOpen = false;
            Parent.CurMiniRoom = this;
            aCurVisitors[0] = Parent.dwId;

            Parent?.SendPacket(GetMiniRoomPacket(Parent));

            FirstTime = false;
        }

        public override void Destroy()
        {
            VisitorKickAll(MR_LeaveResult.Closed);

            Field.Broadcast(MakeLeaveFieldPacket());

            Parent.CurMiniRoom = null;
        }

        public override void Refresh(Character c) => c.SendPacket(Refresh(c.Stats.nMoney));

        // --------------------------------------------------- Handlers

        public override void HandleVisit(CInPacket p, WvsGameClient c) // 0x04
        {
            if (c.Character.dwId == OwnerID)
            {
                c.Character.SendMessage("Handle maintenance");
                HandleMaintenance();
            }
            else if (InBlackList(c.Character))
            {
                c.SendPacket(CPacket.SystemMessage("The owner of this room has blocked you."));
            }
            else if (!ShopOpen)
            {
                c.SendPacket(CPacket.SystemMessage("The shop is currently undergoing maintenance."));
            }
            else if (!HasSpace)
            {
                c.SendPacket(CPacket.SystemMessage("The shop is currently at max capacity. Please try again later."));
            }
            else
            {
                VisitorAdd(c.Character);
            }
        }

        public override void HandlePlayerExit(Character c, MR_LeaveResult nReason = MR_LeaveResult.UserRequest)
        {
            c.CurMiniRoom = null;

            if (aCurVisitors.Contains(c.dwId))
            {
                c.SendMessage("Index Exit: " + GetVisitorIndex(c.dwId));

                aCurVisitors[GetVisitorIndex(c.dwId)] = 0;
                c.SendPacket(PlayerExitPacket(c.dwId, nReason));

                if (OwnerID == c.dwId)
                {
                    aCurVisitors[0] = 0; // hax
                    nCurUsers = 0;
                    ShopOpen = true;
                }
                else
                {
                    nCurUsers = (byte)Math.Max(0, nCurUsers - 1);
                }
            }
        }

        public override void HandleChat(Character pChar, string sMsg)
        {
            sMsg = $"{pChar.Stats.sCharacterName} : {sMsg}";

            var nIndex = GetVisitorIndex(pChar.dwId);

            aChatText.AddLast(new KeyValuePair<string, byte>(sMsg, nIndex));

            VisitorBroadcast(Chat_UserChat(nIndex, sMsg));
        }

        public void HandleMaintenance()
        {
            VisitorKickAll(MR_LeaveResult.StartManage);
            ShopOpen = false;
            Parent.CurMiniRoom = this;
            aCurVisitors[0] = Parent.dwId;
            Parent?.SendPacket(GetMiniRoomPacket(Parent));
        }

        // --------------------------------------------------- shop inventory operations

        public void WithdrawAll()
        {
            // todo
        }

        public void AddItem(CInPacket p)
        {
            var nTI = (InventoryType)p.Decode1();
            var nPOS = p.Decode2();
            var nBundles = Math.Max((short)1, p.Decode2());
            var nPricePerBundle = Math.Max((short)1, p.Decode2());
            var nPrice = Math.Max(1, p.Decode4());

            var pItem = InventoryManipulator.GetItem(Parent, nTI, nPOS);

            if (pItem is null)
            {
                Parent.SendMessage("item is null u fucc");
                return;
            }

            var nQuantityRequested = -1; // negative so its removed completely from inventory

            if (!ItemConstants.is_treat_singly(pItem.nItemID))
            {
                nQuantityRequested = nBundles * nPricePerBundle;

                if (pItem.nNumber < nQuantityRequested)
                {
                    return; // fucking retard
                }
            }

            InventoryManipulator.RemoveFrom(Parent, nTI, nPOS, (short)nQuantityRequested);
            Inventory.Add(new TempItem(pItem, nPrice, nPricePerBundle, nBundles));

            Refresh(Parent);

            Parent.SendMessage($"Bundles: {nBundles}, PricePerBundle: {nPricePerBundle}, Price: {nPrice}");

            // TODO packet editing checks
        }

        public void RemoveItem(CInPacket p)
        {
            // Recv [CP_MiniRoom] [90 00] [26] 01 00

            var nShopSlot = p.Decode2();

            var tItem = Inventory.GetAndRemove(nShopSlot);

            if (tItem is null)
            {
                // do nothing
            }
            else if (InventoryManipulator.CountFreeSlots(Parent, tItem.Item.InvType) <= 0)
            {
                Parent.SendMessage("Please make room in your inventory.");
            }
            else
            {
                InventoryManipulator.InsertInto(Parent, tItem.Item);
            }

            Refresh(Parent);
        }

        public void SellItem(Character c, CInPacket p)
        {
            var nShopSlot = p.Decode1();
            var nQuantity = Math.Max((byte)1, p.Decode2());

            var pItem = Inventory.GetItemInSlot(nShopSlot);

            if (pItem is null || pItem.Item is null)
            {
                return; // some kind of fuckery is going on
            }

            var nFinalQuantity = nQuantity * pItem.ItemsInBundle;
            var nFinalPrice = nFinalQuantity * pItem.Price;

            if (nFinalPrice > c.Stats.nMoney)
            {
                c.SendMessage("no monies");
                return;
            }

            if (nFinalQuantity > pItem.NumberOfBundles * pItem.ItemsInBundle)
            {
                c.SendMessage("no items");
                return;
            }

            if (InventoryManipulator.CountFreeSlots(c, pItem.Item.InvType) <= 0)
            {
                c.SendMessage("no space make space pls");
                return;
            }

            GW_ItemSlotBase iToAdd;

            if (pItem.Item is GW_ItemSlotEquip)
            {
                iToAdd = pItem.Item;
                pItem.NumberOfBundles = 0;
            }
            else
            {
                iToAdd = pItem.Item.DeepCopy();
                iToAdd.nNumber = (short)nFinalQuantity;

                pItem.NumberOfBundles -= (short)nFinalQuantity;
            }

            c.Modify.GainMeso(-nFinalPrice);
            c.Stats.MerchantMesos += GetTax(nFinalPrice) + nFinalPrice;
            InventoryManipulator.InsertInto(c, iToAdd);

            ItemsSold.Add(new SoldItemData
            {
                BuyerName = c.Stats.sCharacterName,
                nItemID = iToAdd.nItemID,
                Quantity = (short)nFinalQuantity,
                Price = nFinalPrice
            });

            Parent?.SendMessage($"Player '{c.Stats.sCharacterName}' has purchased an item from your shop for {nFinalPrice} mesos! You have {pItem.NumberOfBundles} quantity remaining.");
        }

        public void ArrangeItems()
        {
            // todo remove sold items, refresh item list, send notification, retrieve sales fees
        }

        public void DeliverVisitList() => Parent.SendPacket(VisitList());
        public void DeliverBlackList() => Parent.SendPacket(BlackList());

        public void AddBlackList(CInPacket p)
        {
            var sNameToAdd = p.DecodeString();

            for (int i = 0; i < nMaxUsers; i++)
            {
                var pVisitor = MasterManager.CharacterPool.Get(aCurVisitors[i]);

                if (pVisitor?.Stats.sCharacterName.EqualsIgnoreCase(sNameToAdd) ?? false)
                {
                    HandlePlayerExit(pVisitor, MR_LeaveResult.Kicked);
                    break;
                }
            }

            pBlackList.Add(sNameToAdd);
        }

        public void DeleteBlackList(CInPacket p)
        {
            var sNameToRemove = p.DecodeString();
            pBlackList.Remove(sNameToRemove);

            // do we respond with anything???
        }

        // --------------------------------------------------- blacklist

        //public void BanPlayer(string target)
        //{
        //    var dwCharId = MasterManager.CharacterPool
        //        .ToArray()
        //        .FirstOrDefault(cc => cc.Stats.sCharacterName == target)
        //        .dwId;

        //    VisitorKick(dwCharId, SystemMessage(5, 1));
        //    pBlackList.Add(dwCharId);
        //}

        public bool InBlackList(Character target)
            => pBlackList.Contains(target.Stats.sCharacterName);

        // --------------------------------------------------- Visitor stuff

        private void VisitorBroadcast(COutPacket outP)
        {
            using (outP)
            {
                foreach (var item in aCurVisitors)
                {
                    var pChar = MasterManager.CharacterPool.Get(item);
                    pChar?.SendPacket(outP);
                }
            }
        }

        private byte EmptySlot()
        {
            for (byte i = 1; i < nMaxUsers; i++)
            {
                if (aCurVisitors[i] == 0)
                    return i;
            }

            return 0; // full
        }

        private bool HasSpace
            => EmptySlot() != 0;

        private void VisitorAdd(Character c)
        {
            if (aCurVisitors.Contains(c.dwId))
            {
                c.SendMessage("Already in the miniroom.");
                c.CurMiniRoom.HandlePlayerExit(c, MR_LeaveResult.UserRequest);
                return;
            }

            var slot = EmptySlot();

            aCurVisitors[slot] = c.dwId;

            nCurUsers += 1;

            c.CurMiniRoom = this;

            VisitorBroadcast(AvatarPacket(c, slot));

            pVisitList.AddLast(new KeyValuePair<string, int>(c.Stats.sCharacterName, 0)); // unsure what the integer is rn

            c.SendPacket(GetMiniRoomPacket(c));
        }

        private void VisitorKickAll(MR_LeaveResult nReason)
        {
            foreach (var item in aCurVisitors)
            {
                var pChar = MasterManager.CharacterPool.Get(item);

                if (pChar is object)
                {
                    HandlePlayerExit(pChar, nReason);
                }
            }
        }

        public byte GetVisitorIndex(int dwCharId) => (byte)Array.FindIndex(aCurVisitors, aId => aId == dwCharId);
        public int GetValueFromIndex(byte nIndex) => aCurVisitors?[nIndex] ?? 0;

        // --------------------------------------------------- other

        // gets shop sale tax
        public int GetTax(int totalAmount)
        {
            if (totalAmount >= 10000000)
            {
                return (int)(totalAmount * 0.02);
            }
            if (totalAmount >= 5000000)
            {
                return (int)(totalAmount * 0.015);
            }
            if (totalAmount >= 1000000)
            {
                return (int)(totalAmount * 0.01);
            }
            if (totalAmount >= 100000)
            {
                return (int)(totalAmount * 0.005);
            }
            if (totalAmount >= 50000)
            {
                return (int)(totalAmount * 0.0025);
            }
            return 0;
        }

        // --------------------------------------------------- packet stuff


        public COutPacket GetMiniRoomPacket(Character c)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            // CMiniRoomBaseDlg::OnPacketBase
            p.Encode1((byte)MR_Action.MRP_EnterResult);

            // CMiniRoomBaseDlg::OnEnterResultStatic
            p.Encode1((byte)nMiniRoomType); // nMiniRoomType

            // CMiniRoomBaseDlg::OnEnterResultBase
            p.Encode1(nMaxUsers);

            var nIndex = GetVisitorIndex(c.dwId);

            Log.Debug("Index: " + nIndex);

            p.Encode2(nIndex);

            p.Encode4(nTemplateId);
            p.EncodeString(sTitle);

            for (int i = 0; i < nMaxUsers; i++)
            {
                var pVisitor = MasterManager.CharacterPool.Get(aCurVisitors[i]);

                Log.Debug("ID: " + aCurVisitors[i]);

                if (pVisitor is object)
                {
                    EncodeAvatar(p, pVisitor, (byte)(i + 1));
                    Log.Debug("visitor is object");
                }
            }

            // CEntrustedShopDlg::OnEnterResult

            p.Encode1(0xFF); // indicates visitorlist is over

            Parent.SendMessage($"{OwnerID} : {c.dwId}");

            if (OwnerID == c.dwId) // send history if person entering is owner, otherwise no chat history
            {
                p.Encode2((short)aChatText.Count);
                foreach (var (key, value) in aChatText)
                {
                    p.EncodeString(key); // message
                    p.Encode1(value);    // slot
                }
            }
            else
            {
                p.Encode2(0);
            }

            p.EncodeString(OwnerName);

            if (OwnerID == c.dwId)
            {
                p.Encode4(24 * 60 * 60); // time left seconds
                p.Encode1(FirstTime);

                EncodeSoldItemList(p);

                p.Encode8(Parent.Stats.MerchantMesos);
            }

            //////// --->

            p.EncodeString(sTitle);
            p.Encode1(nItemMaxCount);
            p.Encode4(c.Stats.nMoney);

            EncodeItems(p);
            return p;
        }

        public COutPacket PlayerExitPacket(int dwCharId, MR_LeaveResult nReason)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)(dwCharId == OwnerID ? MR_Action.ESP_WithdrawAllResult : MR_Action.MRP_Leave));
            p.Encode1(GetVisitorIndex(dwCharId));
            p.Encode1((byte)nReason);
            return p;
        }

        public COutPacket Chat_GameMessage(MR_Action nMsgCode, string sMsg)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.MRP_GameMessage);
            p.Encode1((byte)nMsgCode);
            p.EncodeString(sMsg);

            return p;
        }

        public COutPacket Chat_UserChat(byte nIndex, string sMsg)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.MRP_Chat);
            p.Encode1((byte)MR_Action.MRP_UserChat);
            p.Encode1(nIndex);
            p.EncodeString(sMsg);

            return p;
        }

        private COutPacket AvatarPacket(Character pChar, byte nSlot)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.MRP_Enter); // visit op
            p.Encode1(nSlot);
            pChar.GetLook().Encode(p);
            p.EncodeString(pChar.Stats.sCharacterName);
            p.Encode2(0);
            return p;
        }

        private COutPacket Refresh(int nMoney)
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);
            p.Encode1((byte)MR_Action.PSP_Refresh);
            p.Encode4(nMoney);
            EncodeItems(p);
            return p;
        }

        private COutPacket BlackList()
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);
            p.Encode1((byte)MR_Action.ESP_DeliverBlackList);
            p.Encode2((short)pBlackList.Count);
            foreach (var item in pBlackList)
            {
                p.EncodeString(item);
            }
            return p;
        }

        private COutPacket VisitList()
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);
            p.Encode1((byte)MR_Action.ESP_DeliverVisitList);
            p.Encode2((short)pVisitList.Count);
            foreach (var item in pVisitList)
            {
                p.EncodeString(item.Key);
                p.Encode4(item.Value); // unsure what this is rn
            }
            return p;
        }

        public void EncodeItems(COutPacket p)
        {
            var nCount = (byte)Inventory.GetAll().Count;
            p.Encode1(nCount);

            if (nCount <= 0)
            {
                p.Encode1(0);
                return;
            }

            foreach (var item in Inventory.GetAll())
            {
                var bundles = 1;

                if (item.Item is GW_ItemSlotBundle isb)
                {
                    bundles = item.NumberOfBundles; // there is some sort of extra handling here but fuck that rn
                }

                p.Encode2(item.NumberOfBundles);
                p.Encode2(item.ItemsInBundle);
                p.Encode4(item.Price);
                item.Item.RawEncode(p);
            }
        }

        public void EncodeSoldItemList(COutPacket p)
        {
            p.Encode1((byte)ItemsSold.Count);
            foreach (var item in ItemsSold)
            {
                p.Encode4(item.nItemID);
                p.Encode2(item.Quantity);
                p.Encode4(item.Price);
                p.EncodeString(item.BuyerName);
            }
        }

        public override bool Expired()
        {
            return false; // todo
        }
    }
}
