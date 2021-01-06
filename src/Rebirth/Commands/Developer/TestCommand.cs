using System;
using Rebirth.Characters.Modify;
using Rebirth.Characters.Skill;
using Rebirth.Common.Types;
using Rebirth.Entities.Item;
using Rebirth.Entities.Shop;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;

namespace Rebirth.Commands.Developer
{
	public sealed class TestCommand : Command
	{
		public override string Name => "test";
		public override string Parameters => string.Empty;
		public override bool IsRestricted => true;
		public override bool IsDisabled => false;

		public override void Execute(CommandCtx ctx)
		{
			if (ctx.Empty)
			{
				ctx.Character.SendMessage("!test <ftype | event | makerskill>");
			}
			else
			{
				switch (ctx.NextString().ToLowerInvariant())
				{
					case "instance":
						ctx.Character.SendMessage($"Instance: {ctx.Character.Field.nInstanceID}");
						break;
					case "potential":
						if (ctx.Empty) return;

						var itemid = ctx.NextInt();

						var pots = new int[3];

						var i = 0;
						while (!ctx.Empty)
						{
							pots[i] = ctx.NextInt();
							i += 1;
						}

						var pItem = MasterManager.CreateItem(itemid, false) as GW_ItemSlotEquip;

						if (pItem is null) return;

						pItem.nGrade = PotentialGradeCode.Visible_Unique;

						pItem.nOption1 = (short)pots[0];
						pItem.nOption2 = (short)pots[1];
						pItem.nOption3 = (short)pots[2];

						InventoryManipulator.InsertInto(ctx.Character, pItem);

						break;
					case "sheepranch":
						ctx.Character.Action.SetFieldInstance(BattlefieldData.BattleMap, 1);
						break;
					case "sethorns":
						ctx.Character.Buffs.AddSkillBuff((int)Skills.BOWMASTER_SHARP_EYES, 30);
						ctx.Character.Buffs.AddSkillBuff((int)Skills.DUAL5_THORNS_EFFECT, 30);
						break;
					case "makerskill":
						ctx.Character.Skills.Add(new SkillEntry(1007) { nSLV = 1 });
						break;
					case "ftype":
						ctx.Character.SendMessage("Map Field Type: " + Enum.GetName(typeof(FieldType), ctx.Character.Field.Template.FieldType));
						break;
					case "event":
						MasterManager.EventManager.TryDoEvent(true);
						break;
					case "shop":
						{
							var pShop = new CShop(69);

							pShop.Items.Add(new CShopItem(0));

							pShop.AddDefaultItems();

							MasterManager.ShopManager.InitUserShop(ctx.Character, 9900000, pShop);
						}
						break;
					case "balloon":
						ctx.SendPacket(CPacket.BalloonMessage("Hello World!", 96));
						break;
					default:
						ctx.Character.SendMessage("No test command with that argument.");
						break;
				}
			}

			return;
			foreach (var mob in ctx.Character.Field.Mobs)
			{
				ctx.Character.SendMessage($"{mob.nMobTemplateId}");
			}
			return;
			int imob = 0;
			foreach (var mob in ctx.Character.Field.Mobs.aMobGen) if (mob.FH == 0) imob += 1;
			ctx.Character.SendMessage($"Mobs with FH 0: {imob}");
			ctx.Character.SendMessage($"Mobs In Map: {ctx.Character.Field.Mobs.Count} || Spawns: {ctx.Character.Field.Mobs.aMobGen.Count}");
			return;
			//foreach(var reactor in ctx.Character.Field.Reactors)
			//{
			//	reactor.IncreaseReactorState(0, 0);
			//}

			//return;
			ctx.Character.Action.SetFieldInstance(240060200, Constants.Rand.Next(), 0);

			return;
			var drop = new CDrop(ctx.Character.Position, ctx.Character.dwId)
			{
				ItemId = 100
			};

			drop.Position.X = drop.StartPosX;
			drop.CalculateY(ctx.Character.Field, drop.StartPosY);

			ctx.Character.Field.Drops.Add(drop);

			return;

			ctx.Character.SendMessage("" + Enum.GetName(typeof(FieldType), ctx.Character.Field.Template.FieldType));

			return;

			var p = new COutPacket(SendOps.LP_Clock);
			var type = ctx.NextInt();
			p.Encode1((byte)type);
			switch (type)
			{
				case 0: // OnEventTimer
					p.Encode4(ctx.NextInt()); // nDuration

					break;
				case 1: // Clock
					p.Encode1((byte)ctx.NextInt()); // nHour
					p.Encode1((byte)ctx.NextInt()); // nMin
					p.Encode1((byte)ctx.NextInt()); // nSec

					break;
				case 2: // Timer
					p.Encode4(ctx.NextInt()); // tDuration (0 to disable the clock)

					break;
				case 3: // some kind of event timer also
					p.Encode1((byte)ctx.NextInt()); // bool (on/off)
					p.Encode4(ctx.NextInt()); // tDuration

					break;
				case 100: // cakepie event timer
					p.Encode1((byte)ctx.NextInt()); // bool (timer on/off)
					p.Encode1((byte)ctx.NextInt()); // nTimerType
					p.Encode4(ctx.NextInt()); // tDuration

					break;
			}

			ctx.Character.SendPacket(p);

			for (int i = 0; i < 2; i++)
			{
				var p2 = new COutPacket(SendOps.LP_CakePieEventResult);
				p2.Encode1(1); // bool -> continue while loop
				p2.Encode4(ctx.Character.Field.MapId); // fieldid
				p2.Encode4(4220176); // itemid
				p2.Encode1(25); // percentage
				p2.Encode1((byte)ctx.NextInt()); // eventstatus
				p2.Encode1((byte)(i + 1)); // nwinnerteam
				p2.Encode1(0); // end while loop

				ctx.SendPacket(p2);
			}

			return;

			//var poo = ctx.Character.Pets.Pets[0];

			//if (poo != null)
			//{
			//	ctx.Character.SendMessage($"{poo.Position.X} || {poo.Position.Y}");
			//}

			//var p = new COutPacket(SendOps.LP_AvatarMegaphoneUpdateMessage);
			//var x = new AvatarMegaphone
			//{
			//	nItemID = 05390000,
			//	sName = "pooodop",
			//	sMsgs = new string[5] { "one", "  two", " three", "    four", " five" },
			//	nChannelNumber = 1,
			//	bWhisper = true,
			//	alSender = ctx.Character.GetLook()
			//};

			//x.Encode(p);

			//MasterManager.CharacterPool.Broadcast(p);

			//var nItemID = ctx.NextInt();

			//ctx.Character.Modify.Inventory(inv =>
			//{
			//	for (short i = 1; i <= ctx.Character.InventoryEquip.SlotLimit; i++)
			//	{
			//		ctx.Character.InventoryEquip.Remove(i);
			//		var newItem = MasterManager.CreateItem(nItemID) as GW_ItemSlotEquip;
			//		newItem.nGrade = PotentialGradeCode.Visible_Unique;
			//		newItem.nOption1 = (int)PotentialLineIDs.LEARN_SKILL_HASTE;
			//		ctx.Character.InventoryEquip.Add(i, newItem);
			//		inv.Add(InventoryType.Equip, i, newItem);
			//	}

			//	var magnifyingGlass = MasterManager.CreateItem(02460003) as GW_ItemSlotBundle;
			//	magnifyingGlass.nNumber = 500; // pretty sure i can override the 100 slotmax lol
			//	ctx.Character.InventoryConsume.Remove(15);
			//	ctx.Character.InventoryConsume.Add(15, magnifyingGlass);

			//	inv.Remove(InventoryType.Consume, 15);
			//	inv.Add(InventoryType.Consume, 15, magnifyingGlass);

			//});

			//return;

			//foreach (var item in ctx.Character.InventoryEquip)
			//{
			//	ctx.Character.Action.ConsumeItem.UseMagnifyingGlass(15, item.Key);
			//	item.Value.sTitle = $"{item.Value.nOption1}|{item.Value.nOption2}";
			//}

			//ctx.Character.InventoryEquip.RemoveAllItems(ctx.Character); // gotta cc for this to take effect so we can look then cc to clean inv

			// ctx.Character.SendPacket(CPacket.UserOpenUI((UIWindow)ctx.NextInt()));

			//ctx.Character.Field.bPauseSpawn = !ctx.Character.Field.bPauseSpawn;

			//ctx.Character.Modify.Stats(mod => mod.SP += (short)ctx.NextInt());

			//var p = new COutPacket(SendOps.LP_UserEffectLocal);

			//for (int i = 8; i >= 1; i--)
			//{
			//    p.Encode4(i == statup.getPosition() ? statup.getValue() : 0);
			//}

			//p.Encode1(1); // skill use
			//p.Encode4(22160000); // skillID
			//p.Encode1(100); // nCharLevel
			//p.Encode1(1); // nSLV
			//p.Encode1(1);

			//ctx.SendPacket(p);

			//ctx.Character.SendMessage($"You have {ctx.Character.Buffs.Count} buffs.");

			//var pChar = ctx.Character;

			//if (pChar.PlayerGuild == null)
			//{
			//    var name = ctx.NextString();
			//    MasterManager.GuildManager.Add(new Guild(name, pChar.dwId));
			//}
			//else
			//{
			//    var p = new COutPacket(SendOps.LP_GuildResult);
			//    p.Encode1((byte)ctx.NextInt());

			//    pChar.SendPacket(p);
			//}
			//var storage = ServerApp.Container.Resolve<CenterStorage>();
			//var sub = storage.Multiplexer().GetSubscriber();

			//sub.Publish("donate", $"{character.AccId}/5000");

			// character.Modify.GainNX(1000);

			// character.SendPacket(CPacket.OpenClassCompetitionPage());

			//var stat = new SecondaryStat();

			//var entry = new SecondaryStatEntry()
			//{
			//    nValue = 10,
			//    rValue = 2001002,
			//    tValue = 110000,
			//};

			//stat.Add(SecondaryStatType.MagicGuard, entry);
			//stat.Add(SecondaryStatType.Flying, entry);

			//character.SendPacket(CPacket.TemporaryStatSetLocal(stat));
			//character.Field.Broadcast(CPacket.UserTemporaryStatSet(stat, character.CharId));

			//var summon = new CSummon();
			//summon.Parent = character;
			//summon.dwSummonedId = 2330810;
			//summon.nSkillID = 14001005;

			//summon.Position.Position.X = character.Position.Position.X;
			//summon.Position.Position.Y = character.Position.Position.Y;
			//summon.Position.Foothold = character.Position.Foothold;

			//summon.nCharLevel = character.Stats.nLevel;
			//summon.nSLV = 1;
			//summon.bMoveAbility = 1;
			//summon.bAssistType = 1;
			//summon.nEnterType = 1;

			//character.Summons.Add(summon);

			//character.SendPacket(CPacket.TradeMoneyLimit(true));
			//character.SendPacket(CPacket.WarnMessage("YEOOOOOO THIS IS A WARNING BOY"));
			//character.SendPacket(CPacket.AdminShopCommodity(9900000));
			//character.SendPacket(CPacket.StandAloneMode(true));
			//character.SendPacket(CPacket.LogoutGift());

			//character.SendPacket(CPacket.UserHireTutor(true));
			//character.SendPacket(CPacket.UserTutorMsg(Constants.ServerMessage2,400,60000));

			//character.SendPacket(CPacket.HontaleTimer(0,5));
			//character.SendPacket(CPacket.HontaleTimer(1, 5));
			//character.SendPacket(CPacket.HontaleTimer(2, 5));
			//character.SendPacket(CPacket.HontaleTimer(3, 5));

			//character.SendPacket(CPacket.HontailTimer(0,69));
			//character.SendPacket(CPacket.HontailTimer(1, 69));
			//character.SendPacket(CPacket.HontailTimer(0, 0));

			//var town = new CTownPortal();
			//town.dwCharacterID = character.dwId;
			//town.Position.X = character.Position.X;
			//town.Position.Y = character.Position.Y;

			//character.Field.TownPortals.Add(town);

			//character.SendPacket(CPacket.DragonEnterField(character.dwId, character.Position.X, character.Position.Y, 0, 2218));
			//character.Dragon = new CDragon(character);
			//character.Dragon.JobCode = 2218;
			//character.Dragon.Position.X = character.Position.X;
			//character.Dragon.Position.Y = character.Position.Y;


			//    var gate1 = new COpenGate()
			//    {
			//        dwCharacterID = character.dwId,
			//    };

			//    gate1.Position.X = character.Position.X;
			//    gate1.Position.Y = character.Position.Y;

			//    character.Field.OpenGates1.Add(gate1);

			//    var gate2 = new COpenGate()
			//    {
			//        dwCharacterID = character.dwId,
			//    };

			//    gate2.Position.X = character.Position.X;
			//    gate2.Position.X -= 250;

			//    gate2.Position.Y = character.Position.Y;

			//    character.Field.OpenGates2.Add(gate2);


			//character.SendPacket(CPacket.StageChange("ThemeBack1.img", 0));

			//character.Field.Employees.RemoveAt(0);

			//character.SendPacket(CPacket.SetBackgroundEffect(2, 100000000, 1, 30 * 1000));

			//character.SendPacket(CPacket.Desc(true));

			//character.SendPacket(CPacket.StalkResult(character.dwId, character.Stats.sCharacterName, character.Position.X, character.Position.Y));

			//character.SendPacket(CPacket.PlayJukeBox("Hydromorph"));
			//character.SendPacket(CPacket.BlowWeather(0, 5120000, "Hello Twitch!!!"));

			//character.SendPacket(CPacket.AntiMacroResult(6, "SnitchNigga")); //Crash
			//character.SendPacket(CPacket.AntiMacroResult(7, "SnitchNigga")); //This dude reported you !

			//Diemension mirror reeeeeeeeeeeeeee
			//var p = NpcScript.ScriptMessageHeader(0, 9900000, ScriptMsgType.AskSlideMenu);
			//p.Encode4(0); //dlg type
			//p.Encode4(0); //index?
			//p.EncodeString("Hello World");
			//character.SendPacket(p);

			//var pet = new CPet(ctx.Character);
			//pet.nIdx = 0;
			//pet.liPetLockerSN = 9001;
			//pet.dwTemplateID = 5000102;
			//pet.sName = "Charlie";
			//pet.Position.X = character.Position.X;
			//pet.Position.Y = character.Position.Y;

			//ctx.Character.Pets.Add(pet.liPetLockerSN, pet);
		}
	}
}
