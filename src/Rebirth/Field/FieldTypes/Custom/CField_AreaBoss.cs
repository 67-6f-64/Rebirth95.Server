using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebirth.Common.Tools;
using Rebirth.Common.Types;
using Rebirth.Server.Game;
using Rebirth.Tools;

namespace Rebirth.Field.FieldTypes.Custom
{
	public sealed class CField_AreaBoss : CField
	{
		private readonly DateTime _timeSinceCreation;

		public CField_AreaBoss(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId)
		{
			_timeSinceCreation = DateTime.Now;
			//#if DEBUG
			//			bPauseSpawn = true;
			//#endif
		}

		public override void Update()
		{
			TrySpawnBoss();

			base.Update();
		}

		private void TrySpawnBoss()
		{
#if RELEASE
			if (_timeSinceCreation.SecondsSinceStart() <= 2 * 60) return; // need to be in map for 2 mins before the boss will spawn
#endif
			if (!Users.Any()) return;

			if (!ParentInstance.AreaBossManager.CanSpawn(MapId)) return;

			TagPoint pt;
			int mobId;

			switch (MapId)
			{
				case 104010200: // forest trail 2 - mano
					pt = new TagPoint(308, 515);
					mobId = 2220000;
					break;
				case 100020101: // mushmom forest trail - mushmom
					pt = new TagPoint(-691, 215);
					mobId = 6130101;
					break;
				case 100020301: // blue mushmom forest - blue mushmom
					pt = new TagPoint(158, -685);
					mobId = 8220007;
					break;
				case 102020500: // gusty peak - stumpy
					pt = new TagPoint(549, 2168);
					mobId = 3220000;
					break;
				case 103030300: // unseen danger - dyle
					pt = new TagPoint(99, 119);
					goto case 103030400;
				case 103030400: // deep mire - dyle
					pt = new TagPoint(541, 121);
					mobId = 0;
					break;
				case 211041400: // forest of dead trees IV - riche
					pt = new TagPoint(1278, 94);
					mobId = 6090000;
					break;
				case 211040500: // sharp cliff III - Snowman 
					pt = new TagPoint(37, 62);
					mobId = 8220001;
					break;
				case 211050000: // icy cold field - snow witch
					pt = new TagPoint(288, -146);
					mobId = 6090001;
					break;
				case 200010302: // garden of darkness II - eliza
					pt = new TagPoint(236, 83);
					mobId = 8220000;
					break;
				case 105020400: // cave exit - snack bar
					pt = new TagPoint(631, 350);
					mobId = 8220008;
					break;
				case 230020100: // seaweed tower - seruf
					pt = new TagPoint(-36, -80);
					mobId = 4220001;
					break;
				case 221020701: // eos tower 4th floor - rombot
					pt = new TagPoint(-60, 1513);
					mobId = 4130103;
					break;
				case 220050200: // lost time 2 - timer
					pt = new TagPoint(395, 1032);
					mobId = 5220003;
					break;
				//case 551030200: // spooky world - scarga
				//	pt = new TagPoint();
				//	mobId = 0;
				//	break;
				case 240020402: // manons forest - manon
					pt = new TagPoint(-371, 452);
					mobId = 8180000;
					break;
				case 240020102: // griffey forest - griffey
					pt = new TagPoint(422, 452);
					mobId = 8180001;
					break;
				case 240040401: // levtiathans canyon - leviathan
					pt = new TagPoint(-52, 1641);
					mobId = 8220003;
					break;
				case 250020300: // advanced practice field - master dummy
					pt = new TagPoint(1178, 51);
					mobId = 5090001;
					break;
				case 250010304: // Territory of Wandering Bear - Tae Roon
					pt = new TagPoint(-480, 393);
					mobId = 7220000;
					break;
				case 250010504: // Goblin Forest 2 - King Sage Cat
					pt = new TagPoint(76, 543);
					mobId = 7220002;
					break;
				case 251010101: // 60-Year-Old Herb Garden - Bamboo Warrior
					pt = new TagPoint(308, 123);
					mobId = 6090002;
					break;
				case 251010102: // 80-Year-Old Herb Garden - Giant Centipede
					pt = new TagPoint(435, -436);
					mobId = 5220004;
					break;
				case 260010500: // Dry Desert - Deo
					pt = new TagPoint(733, 275);
					mobId = 3220001;
					break;
				case 261010003: // Lab - Unit 103 - Rurumo
					pt = new TagPoint(305, 313);
					mobId = 6090004;
					break;
				case 261020300: // Lab - Area C-1 - Security Camera
					pt = new TagPoint(390, 167);
					mobId = 7090000;
					break;
				case 261030000: // Lab - Secret Basement Path - Chimera
					pt = new TagPoint(-321, 181);
					mobId = 8220002;
					break;
				case 270010500: // Memory Lane 5 - Dodo
					pt = new TagPoint(-264, 181);
					mobId = 8220004;
					break;
				case 270020500: // Road of Regrets 5 - Lilynouch
					pt = new TagPoint(108, -915);
					mobId = 8220005;
					break;
				case 270030500: // Road to Oblivion 5 - Lyka
					pt = new TagPoint(239, -570);
					mobId = 8220006;
					break;
				//case 682000001: // Hollowed Ground - Headless Horseman
				//	pt = new TagPoint(304, 238);
				//	mobId = 9400549;
				//	break;
				//case 610010100: // Twisted Paths - Bigfoot
				//case 610010101:
				//case 610010102:
				//case 610010103:
				//case 610010104:
				//	pt = new TagPoint(526, 208);

				//	// 50/50 to spawn bigfoot or headless horseman
				//	mobId = Constants.Rand.NextDouble() > 0.5 ? 9400549 : 9400575;
				//	break;
				default: return;
			}

			Mobs.CreateMob(mobId, null, pt.X, pt.Y, 0, 0xFE, 0, 1, MobType.Normal, null);

			foreach (var item in Mobs)
			{
				item.SetController(Users.Random(), MobCtrlType.Active_Int);
			}

			Broadcast(CPacket.SystemMessage("The area boss emerges from the depths...."));
		}
	}
}
