using log4net;
using Rebirth.Characters;
using Rebirth.Characters.Modify;
using Rebirth.Client;
using Rebirth.Field.MiniRoom;
using Rebirth.Network;
using System;
using System.Linq;
using Rebirth.Common.Types;
using static Rebirth.Field.MiniRoom.MiniRoomEnum;

namespace Rebirth.Field
{
    public class CMiniRoomPool : CObjectPool<CMiniRoomBase>
    {
        public static ILog Log = LogManager.GetLogger(typeof(CMiniRoomPool));

        public CMiniRoomPool(CField fieldItem) : base(fieldItem) { }

        public void Update()
        {
            // todo remove expired
            // set CurrentStatus to Expired before removing from pool
        }

        public void CreateMiniRoom(Character c, MR_Type nType, CInPacket p)
        {
            switch (nType)
            {
                case MR_Type.Omok:
                    // todo
                    break;
                case MR_Type.MemoryGame:
                    // todo
                    break;
                case MR_Type.TradingRoom:
                    if (c.CurMiniRoom != null)
                    {
                        c.SendPacket(CPacket.SystemMessage("You are already in a trade."));
                    }
                    else
                    {
                        Log.Debug($"Adding trade mini room to field minirooms.");
                        Add(new CTradingRoom(c));
                    }
                    break;

                // Recv [CP_MiniRoom] 90 00 00 05 07 00 70 65 65 66 61 63 65 00 01 00 76 C0 4C 00
                case MR_Type.PersonalShop:
                    c.SendMessage("no");
                    break;
                case MR_Type.EntrustedShop:
                    {
                        // TODO check closeness to other shops
                        var sTitle = p.DecodeString();
                        p.Skip(3); // TODO what is this
                        var nShopTemplateId = p.Decode4();

                        var (nItemSlot, pItem) = InventoryManipulator.GetAnyItem(c, InventoryType.Cash, nShopTemplateId);

                        Log.Info($"Player [{c.Stats.sCharacterName}] attempting to create a shop with ItemID [{nShopTemplateId}] and Title [{sTitle}]");

                        if (pItem == null || nItemSlot == 0)
                        {
                            c.SendPacket(CPacket.SystemMessage("Invalid item or item slot.")); // packet editing?
                        }
                        else if (!c.Field.MapId.InRange(910000000, 910000023))
                        {
                            c.SendPacket(CPacket.SystemMessage("Item does not work in this map."));
                        }
                        else if (!ItemConstants.is_entrusted_shop_item(pItem.nItemID))
                        {
                            c.SendPacket(CPacket.SystemMessage("Invalid shop item.")); // packet editing??
                        }
                        else
                        {
                            var pMiniRoom = new CEntrustedShop(c, nShopTemplateId, sTitle);

                            if (pMiniRoom.HasItems())
                            {
                                c.SendMessage("Please retrieve items from fredrick before opening a shop.");
                            }
                            else
                            {
                                //Add(pMiniRoom); // packet is sent in here
                            }
                        }
                        break;
                    }
                case MR_Type.CashTradingRoom:
                    // todo
                    break;
                default:
                    break;
            }
        }

        public static void EntrustedShopRequest(WvsGameClient c, CInPacket p)
        {
            var opCode = p.Decode1();
            if (opCode == 0)
            {
                var outP = new COutPacket(SendOps.LP_EntrustedShopCheckResult);
                outP.Encode1(0x07);
                c.SendPacket(outP);
            }
        }

        public static void OnPacket(WvsGameClient c, CInPacket p)
        {
            var pChar = c.Character;
            var pMiniRoom = pChar.CurMiniRoom;

            Log.Debug($"Begin Handle_MiniRoom");

            var opcode = p.Decode1();
            Log.Debug($"Operation: {opcode}");

            switch ((MR_Action)opcode)
            {
                case MR_Action.MRP_Create:
                    {
                        var type = p.Decode1();
                        Log.Debug($"Create Type: {type}");
                        c.Character.Field.MiniRooms.CreateMiniRoom(c.Character, (MR_Type)type, p);
                        break;
                    }
                case MR_Action.MRP_Invite: // occurs in some but not all rooms (games, messenger, trade, not shops)
                    {
                        var dwTargetCharId = p.Decode4();
                        Log.Debug($"Processing trade invite request to char ID {dwTargetCharId}");

                        if (pMiniRoom is CTradingRoom room)
                        {
                            room.HandleSendInvite(dwTargetCharId);
                        }

                        break;
                    }
                case MR_Action.MRP_InviteResult:
                    {
                        if (pMiniRoom is CTradingRoom ctr)
                        {
                            ctr.HandleDecline();
                        }

                        break;
                    }
                case MR_Action.MRP_Enter:
                    {
                        var targetRoomId = p.Decode4(); // theres two more bytes after this which im curious to know what they do...
                        // the extra bytes might be fm room?? for remote merchants??

                        Log.Info($"DWID: {targetRoomId}");
                        var room = pChar.Field.MiniRooms.FirstOrDefault(r => r.dwId == targetRoomId);
                        // if remote merchant operations use this same packet process then it will not work because we're searching by field.. we'd need to search by a range of fields in the channel instead
                        if (room is null)
                        {
                            c.SendPacket(FailedEnterPacket());
                        }
                        else
                        {
                            room?.HandleVisit(p, c);
                        }

                        break;
                    }
                case MR_Action.MRP_Chat:
                    {
                        p.Skip(4); // timestamp

                        pChar.CurMiniRoom?.HandleChat(pChar, p.DecodeString());
                        break;
                    }
                case MR_Action.MRP_Leave:
                    {
                        pChar.CurMiniRoom?.HandlePlayerExit(pChar, MR_LeaveResult.UserRequest);
                        break;
                    }
                case MR_Action.ESP_WithdrawAll: // owner close
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces && ces.OwnerID == pChar.dwId)
                        {
                            ces.WithdrawAll();
                            ces.HandlePlayerExit(pChar, MR_LeaveResult.UserRequest);
                            ces.Destroy();
                        }
                        break;
                    }
                case MR_Action.MRP_Balloon: // ?? these names lmao
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces && pChar.CurMiniRoom.OwnerID == pChar.dwId)
                        {
                            // pChar.Field.Broadcast(r.MakeEnterFieldPacket()); // gms spawns the shop after owner fills it with items

                            ces.ShopOpen = true;
                            pChar.CurMiniRoom = null;
                        }
                        break;
                    }
                case MR_Action.TRP_PutItem:
                    {
                        if (pChar.CurMiniRoom is CTradingRoom ctr)
                        {
                            ctr.HandleAddItem(p, pChar);
                        }
                        break;
                    }
                case MR_Action.TRP_PutMoney:
                    {
                        if (pChar.CurMiniRoom is CTradingRoom ctr)
                        {
                            ctr.HandleSetMeso(pChar, p.Decode4());
                        }
                        break;
                    }
                case MR_Action.TRP_Trade:
                    {
                        if (pChar.CurMiniRoom is CTradingRoom ctr)
                        {
                            ctr.HandleConfirmTrade(pChar);
                        }
                        break;
                    }
                case MR_Action.PSP_PutItem:
                case MR_Action.ESP_PutItem:
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces && pChar.CurMiniRoom.OwnerID == pChar.dwId)
                        {
                            ces.AddItem(p);
                        }
                        break;
                    }
                case MR_Action.ESP_BuyItem:
                case MR_Action.PSP_BuyItem:
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces)
                        {
                            ces.SellItem(c.Character, p);
                        }
                        break;
                    }
                case MR_Action.ESP_Refresh:
                case MR_Action.PSP_Refresh:
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces)
                        {
                            ces.Refresh(pChar);
                        }
                        break;
                    }
                case MR_Action.ESP_MoveItemToInventory:
                case MR_Action.PSP_MoveItemToInventory:
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces)
                        {
                            ces.RemoveItem(p);
                        }
                        break;
                    }
                case MR_Action.PSP_Ban:
                case MR_Action.MGRP_Ban:
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces && pChar.CurMiniRoom.OwnerID == pChar.dwId)
                        {
                            //ces.BanPlayer(p.DecodeString());
                        }
                        break;
                    }
                case MR_Action.ESP_ArrangeItem:
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces && pChar.CurMiniRoom.OwnerID == pChar.dwId)
                        {
                            ces.ArrangeItems();
                        }
                        break;
                    }
                case MR_Action.ESP_DeliverVisitList:
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces && pChar.CurMiniRoom.OwnerID == pChar.dwId)
                        {
                            ces.DeliverVisitList();
                        }
                        break;
                    }
                case MR_Action.ESP_DeliverBlackList:
                case MR_Action.PSP_DeliverBlackList:
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces && pChar.CurMiniRoom.OwnerID == pChar.dwId)
                        {
                            ces.DeliverBlackList();
                        }
                        break;
                    }
                case MR_Action.ESP_AddBlackList:
                case MR_Action.PSP_AddBlackList:
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces && pChar.CurMiniRoom.OwnerID == pChar.dwId)
                        {
                            ces.AddBlackList(p);
                        }
                        break;
                    }
                case MR_Action.PSP_DeleteBlackList:
                case MR_Action.ESP_DeleteBlackList:
                    {
                        if (pChar.CurMiniRoom is CEntrustedShop ces && pChar.CurMiniRoom.OwnerID == pChar.dwId)
                        {
                            ces.DeleteBlackList(p);
                        }
                        break;
                    }
                default:
                    Log.Info($"Unhandled MiniRoom packet. OpCode: {opcode}.");
                    Log.Info($"Full packet: {BitConverter.ToString(p.ToArray()).Replace("-", " ")}");
                    break;
            }
            pChar.Action.Enable();
        }

        // ----------------------------------------- end handle incoming miniroom visit packets

        public int ShopCount
            => this.Where(ctx => ctx is CEntrustedShop || ctx is CPersonalShop)
            .ToList()
            .Count;

        protected override void InsertItem(int index, CMiniRoomBase room)
        {
            base.InsertItem(index, room);

            room.Create(); // sends init packets
        }

        protected override void RemoveItem(int index)
        {
            var room = GetAtIndex(index);

            if (!room?.Destroyed ?? false)
            {
                room.Destroy();
            }

            base.RemoveItem(index);
        }

        private static COutPacket FailedEnterPacket()
        {
            var p = new COutPacket(SendOps.LP_MiniRoom);

            p.Encode1((byte)MR_Action.MRP_Leave);
            p.Encode1(0);
            p.Encode1((byte)MR_LeaveResult.Closed);

            return p;
        }

        //CEmployeePool::OnPacket
        public static COutPacket EmployeeEnterField(CMiniRoomBase e)
        {
            var p = new COutPacket(SendOps.LP_EmployeeEnterField);
            p.Encode4(e.OwnerID); // dwEmployerID
            if (e is CEntrustedShop ces)
            {
                p.Encode4(ces.nTemplateId);
            }
            else if (e is CPersonalShop cps)
            {
                p.Encode4(cps.nTemplateId);
            }
            else
            {
                throw new InvalidOperationException("Unknown employee type");
            }
            EncodeEmployeeInit(p, e);
            EncodeEmployeeSetBalloon(p, e);
            return p;
        }

        public static COutPacket EmployeeLeaveField(int dwEmployeeId)
        {
            var p = new COutPacket(SendOps.LP_EmployeeLeaveField);
            p.Encode4(dwEmployeeId);
            return p;
        }

        public static COutPacket EmployeeMiniRoomBalloon(CMiniRoomBase e)
        {
            var p = new COutPacket(SendOps.LP_EmployeeMiniRoomBalloon);
            p.Encode4(e.dwId);
            EncodeEmployeeSetBalloon(p, e);
            return p;
        }

        private static void EncodeLook(int index, COutPacket p, Character c)
        {
            p.Encode1((byte)index);
            c.GetLook().Encode(p);
            p.EncodeString(c.Stats.sCharacterName);
            p.Encode2(c.Stats.nJob);
        }

        private static void EncodeEmployeeInit(COutPacket p, CMiniRoomBase cMiniRoom)
        {
            cMiniRoom.Position.EncodePos(p);
            p.Encode2(cMiniRoom.Position.Foothold);
            p.EncodeString(cMiniRoom.OwnerName);
        }

        private static void EncodeEmployeeSetBalloon(COutPacket p, CMiniRoomBase cMiniRoom)
        {
            p.Encode1((byte)cMiniRoom.nMiniRoomType);
            p.Encode4(cMiniRoom.dwId); //dwMiniRoomSN

            if ((byte)cMiniRoom.nMiniRoomType <= 0)
                return; // idk why this would happen but its in pdb

            if (cMiniRoom is CEntrustedShop ces)
            {
                p.EncodeString(ces.sTitle);
                p.Encode1((byte)(ces.nTemplateId % 10)); // nSpec
            }
            else if (cMiniRoom is CPersonalShop cps)
            {
                p.EncodeString(cps.sTitle);
                p.Encode1((byte)(cps.nTemplateId % 10)); // nSpec
            }
            else
            {
                throw new InvalidOperationException("Unknown employee type");
            }
            p.Encode1(cMiniRoom.nCurUsers);
            p.Encode1(cMiniRoom.nMaxUsers);
        }
    }
}