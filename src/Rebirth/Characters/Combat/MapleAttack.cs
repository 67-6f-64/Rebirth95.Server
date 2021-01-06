using System;
using log4net;
using Newtonsoft.Json.Serialization;
using Rebirth.Characters.Skill;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Tools;
using Rebirth.Common.Types;
using Rebirth.Network;
using Rebirth.Server.Center;
using Rebirth.Tools;

namespace Rebirth.Characters.Combat
{
	public class AttackEntry
	{
		public int dwMobID;

		public byte nHitAction;
		public byte nForeAction;
		public byte nFrameIdx;
		public byte CalcDamageStatIndex;
		public int tDelay;
		public int nAttackCount;
		public readonly int[] aDamage;
		public readonly int[] abCritical;
		public readonly TagPoint ptHit;

		public AttackEntry()
		{
			aDamage = new int[15];
			abCritical = new int[15];
			ptHit = new TagPoint();
		}
	}

	public class MapleAttack
	{
		public static ILog Log = LogManager.GetLogger(typeof(MapleAttack));

		public byte nDamagePerMob { get; set; }
		public byte nMobCount { get; set; }
		public int nSkillID { get; set; }
		public int tKeyDown { get; set; } //"Charge" 
		public int bLeft { get; set; }
		public int nAction { get; set; }
		public int nAttackType { get; set; }
		public int tStart { get; set; }
		public int nSpeedDegree { get; set; }
		public int tAttackTime { get; set; }
		public AttackEntry[] aAttackInfo { get; set; }

		public bool bValidAttack { get; set; }

		public byte nActionSpeed { get; set; }
		public byte nMastery { get; set; }
		/// <summary>
		/// This is assigned by the server and sent in the remote 
		///		attack packet response to indicate what bullet template to show
		/// </summary>
		public int nBulletItemID { get; set; }
		/// <summary>
		/// Sent by the client to indicate the position that the bullet item is in (always Consume)
		/// </summary>
		public short nBulletItemPos { get; set; }
		/// <summary>
		/// Sent by the client to indicate the position (if any) that the cash bullet skin is in (always Cash)
		/// </summary>
		public short nBulletCashItemPos { get; set; }
		/// <summary>
		/// Sent by the client to indicate what bullet ID it is trying to use
		/// </summary>
		public int nSpiritJavelinItemID { get; set; }

		public short nActionAndDir { get; set; }
		/// <summary>
		/// Skill option flag for:
		/// Spark, Spirit Javelin, Serial Attack Skill, Shadow Partner, Slash Blast Final Attack, 
		/// </summary>
		public byte nOption { get; set; }

		public byte nAttackActionType { get; set; }
		public byte nAttackSpeed { get; set; }
		public bool bNextShootExJablin { get; set; }
		public bool bNoItemConsume { get; set; }

		public int tReserveSpark { get; set; }

		public short nGrenadePtX { get; set; }
		public short nGrenadePtY { get; set; }

		public MapleAttack()
		{
			bNoItemConsume = true;
			aAttackInfo = new AttackEntry[15];

			bValidAttack = true; // set to invalid if something goes wrong in parsing -> Review
		}

		public static MapleAttack ParseMelee(CInPacket p, Character c)
		{
			MapleAttack ret = new MapleAttack();

			var nFirstByte = p.Decode1();

			if (nFirstByte == 1 || nFirstByte == 2) // CUserLocal::TryDoingNormalAttack
			{
				p.Skip(8);
			}
			else // CUserLocal::TryDoingMeleeAttack
			{
				p.Skip(7);
			}

			var tByte1 = p.Decode1();
			ret.nDamagePerMob = (byte)(tByte1 & 0xF);
			ret.nMobCount = (byte)((tByte1 >> 4) & 0xF);

			p.Skip(8); //-1

			var v11 = p.Decode4();
			ret.nSkillID = v11;

			ret.ValidateSkill(c);
			if (!ret.bValidAttack)
				return ret;

			ret.bNoItemConsume = true;

			var template = MasterManager.SkillTemplates[ret.nSkillID];

			p.Skip(1);
			p.Skip(8); // 0.74
			p.Skip(8); // 0.88

			ret.tKeyDown = template?.is_keydown_skill ?? false ? p.Decode4() : -1;

			var bSomeFlag = p.Decode1(); // bFinalAfterSlashBlast | 8 * bShadowPartner | 16 * v685 | 32 * (nSerialAttackSkillID != 0) | ((_BYTE)v694 << 7));

			var bSpark = ((bSomeFlag >> 7) & 1) > 0;
			var bSerialAttackSkill = ((bSomeFlag >> 5) & 1) > 0;
			var bShadowPartner = ((bSomeFlag >> 3) & 1) > 0;
			var bFinalAfterSlashBlast = (bSomeFlag & 1) > 0;

			ret.nActionAndDir = p.Decode2();
			ret.bLeft = (ret.nActionAndDir >> 15) & 1;
			ret.nAction = ret.nActionAndDir & 0x7FFF;
			p.Skip(4); //CRC i think
			ret.nAttackActionType = p.Decode1();
			ret.nAttackSpeed = p.Decode1();

			ret.tAttackTime = p.Decode4();
			p.Skip(4); //bmage?

			//More decode for bullets here

			//if (ret.skill == 4211006)
			//{ // Meso Explosion
			//    return parseMesoExplosion(lea, ret);
			//}

			ret.DecodeAttackInfo(p);

			p.Skip(4); // position

			if (ret.nSkillID == (int)Skills.NIGHTWALKER_POISON_BOMB)
			{
				ret.nGrenadePtX = p.Decode2();
				ret.nGrenadePtY = p.Decode2();
			}

			return ret;
		}

		public static MapleAttack ParseMagic(CInPacket p, Character c)
		{
			MapleAttack ret = new MapleAttack();

			p.Skip(8); // -1

			var tByte1 = p.Decode1();
			ret.nDamagePerMob = (byte)(tByte1 & 0xF);
			ret.nMobCount = (byte)((tByte1 >> 4) & 0xF);

			p.Skip(8); //-1

			var v11 = p.Decode4(); //2017
			ret.nSkillID = v11;
			p.Skip(1); // nCombatOrders
			p.Skip(4); // get_rand(pDrInfo.dr0, 0);
			p.Skip(4); // CCrc32::GetCrc32(pData, 4u, n, 0, 0);
			p.Skip(8);
			p.Skip(8);

			p.Skip(4); //dwInit
			p.Skip(4); //COutPacket::Encode4(&v468, dwInit);

			p.Skip(4); //SKILLLEVELDATA Crc ?
			p.Skip(4); //SKILLLEVELDATA Crc ?

			ret.ValidateSkill(c);

			if (!ret.bValidAttack) return ret;

			ret.bNoItemConsume = true;

			if (v11 == 2121001 || v11 == 2221001 || v11 == 2321001 || v11 == 22121000 || v11 == 22151001)
				ret.tKeyDown = p.Decode4();
			else
				ret.tKeyDown = -1;

			ret.nOption = p.Decode1(); // p.Skip(1); // Zero !

			ret.nActionAndDir = p.Decode2();
			ret.bLeft = (ret.nActionAndDir >> 15) & 1;
			ret.nAction = ret.nActionAndDir & 0x7FFF; //nAttackAction
			p.Skip(4); //CRC i think
			ret.nAttackActionType = p.Decode1(); //nAttackSpeed | 16 * nReduceCount
			ret.nAttackSpeed = p.Decode1();

			ret.tAttackTime = p.Decode4();
			p.Skip(4); //nPhase

			ret.DecodeAttackInfo(p);

			//ret.position = p.DecodePos(); //Confirm this           

			return ret;
		}

		public static MapleAttack ParseShoot(CInPacket p, Character c)
		{
			MapleAttack ret = new MapleAttack();

			p.Skip(8); // -1

			var tByte1 = p.Decode1();
			ret.nDamagePerMob = (byte)(tByte1 & 0xF);
			ret.nMobCount = (byte)((tByte1 >> 4) & 0xF);

			//c.SendMessage("Damage per mob " + ret.nDamagePerMob);
			//c.SendMessage("Mob count " + ret.nMobCount);

			p.Skip(8); //-1

			ret.nSkillID = p.Decode4(); //2017

			ret.ValidateSkill(c);

			if (!ret.bValidAttack) return ret;

			var skillTemplate = MasterManager.SkillTemplates[ret.nSkillID];

			var nCombatOrders = p.Decode1(); // nCombatOrders
			p.Skip(4); // get_rand(pDrInfo.dr0, 0);
			p.Skip(4); // CCrc32::GetCrc32(pData, 4u, n, 0, 0);
			p.Skip(8);

			ret.tKeyDown = skillTemplate?.is_keydown_skill ?? false ? p.Decode4() : -1;

			ret.nOption = p.Decode1(); //COutPacket::Encode1(&v498, (bSpark << 7) | (bSpiritJavelin << 6) | 8 * bShadowPartner | 4 * bMortalBlow | 2 * bSoulArrow);

			var bSpark = (ret.nOption & 0x80) != 0;
			var bSpiritJavelin = (ret.nOption & 0x40) != 0;
			var bSerial = (ret.nOption & 0x20) != 0;
			var bShadowPartner = (ret.nOption & 0x8) != 0;
			var bMortalBlow = (ret.nOption & 0x4) != 0;
			var bSoulArrow = (ret.nOption & 0x2) != 0;

			ret.bNextShootExJablin = p.Decode1() > 0; //v339->m_bNextShootExJablin && CUserLocal::CheckApplyExJablin(v339, pSkill, nAttackAction);

			ret.nActionAndDir = p.Decode2();
			ret.bLeft = (ret.nActionAndDir >> 15) & 1;
			ret.nAction = ret.nActionAndDir & 0x7FFF; //nAttackAction
			p.Skip(4); //CRC i think
			ret.nAttackActionType = p.Decode1(); //nAttackSpeed | 16 * nReduceCount
			ret.nAttackSpeed = p.Decode1();

			ret.tAttackTime = p.Decode4();
			p.Skip(4); //nPhase

			ret.nBulletItemPos = p.Decode2();
			ret.nBulletCashItemPos = p.Decode2();
			var nShootRange0a = p.Decode1(); // nSLV (BMS)

			ret.bNoItemConsume = skillTemplate?.is_shoot_skill_not_consuming_bullet ?? false;

			if (!ret.bNoItemConsume)
			{
				if (JobLogic.is_mechanic_job(c.Stats.nJob))
				{
					ret.bNoItemConsume = true;
				}
				else if (bSoulArrow)
				{
					ret.bNoItemConsume = c.Buffs[(int)Skills.HUNTER_SOUL_ARROW_BOW] != null
					   || c.Buffs[(int)Skills.CROSSBOWMAN_SOUL_ARROW_CROSSBOW] != null
					   || c.Buffs[(int)Skills.WINDBREAKER_SOUL_ARROW_BOW] != null
					   || c.Buffs[(int)Skills.WILDHUNTER_SOUL_ARROW_CROSSBOW] != null;
				}
				else if (bSpiritJavelin)
				{
					if (ret.bNextShootExJablin)
					{
						ret.bNextShootExJablin = false;
					}
					else
					{
						ret.nSpiritJavelinItemID = p.Decode4();

						var nBulletType = (ret.nSpiritJavelinItemID % 10000) + 1;

						c.Buffs.TryGetValue((int)Skills.NIGHTLORD_SPIRIT_JAVELIN, out AbstractBuff secondaryStat_nSpiritJavelin);

						if (c.InventoryConsume.Get(ret.nBulletItemPos)?.nItemID != ret.nSpiritJavelinItemID)
						{
							c.Buffs.Remove((int)Skills.NIGHTLORD_SPIRIT_JAVELIN); // they have to use the same item as they 
						}
						else
						{
							if (secondaryStat_nSpiritJavelin != null && nBulletType != secondaryStat_nSpiritJavelin.Stat[SecondaryStatFlag.SpiritJavelin].nValue)
							{
								ret.bValidAttack = false; // PE
								return ret;
							}

							ret.bNoItemConsume = true;
						}
					}
				}
				else if (ret.bNextShootExJablin)
				{
					ret.bNoItemConsume = true;
				}
			}

			ret.DecodeAttackInfo(p);

			p.Skip(4); // char pos

			if (JobLogic.IsWildhunterJob(c.Stats.nJob))
			{
				var m_ptBodyRelMove = p.Decode2(); // unsure what this is used for
			}

			p.Skip(4); // another pos.. unsure for what

			if (ret.nSkillID == (int)Skills.STRIKER_SPARK)
			{
				ret.tReserveSpark = p.Decode4();
			}

			return ret;
		}

		public static MapleAttack ParseBody(CInPacket p, Character c)
		{
			return ParseMelee(p, c); //TODO: Confirm below one day. Delta said melee works for this 

			// Recv [CP_UserBodyAttack] [32 00] [01] [FF FF FF FF] [FF FF FF FF] [11] [FF FF FF FF] [FF FF FF FF] [CB F9 41 01] [00] [C3 C0 24 6A] [1F 46 7E D1] [9D D4 DD D5] [9D D4 DD D5] [00 00] 80 5F CF 88 C3 02 00 F9 9E D9 1F 00 00 00 00 2F 27 00 00 07 00 01 05 A9 00 8B 01 A5 00 8B 01 00 00 0B 00 00 00 B8 8D DB 27 B9 00 8B 01

			MapleAttack ret = new MapleAttack();

			p.Decode1(); // field key
			p.Decode4(); // pDrInfo.dr0
			p.Decode4(); // pDrInfo.dr1

			ret.nDamagePerMob = (byte)(p.Decode1() & 0xF); // nDamagePerMob | 0x10 * nRange

			p.Decode4(); // pDrInfo.dr2
			p.Decode4(); // pDrInfo.dr3

			ret.nSkillID = p.Decode4();

			ret.ValidateSkill(c);
			if (!ret.bValidAttack)
				return ret;

			p.Decode1(); // cd->nCombatOrders
			p.Decode4(); // get_rand(pDrInfo.dr0, 0)
			p.Decode4(); // CCrc32::GetCrc32(pData, 4u, n, 0, 0)

			// todo lol

			return null;
		}

		/// <summary>
		/// TODO spin this off into a separate class
		/// </summary>
		/// <param name="c"></param>
		private void ValidateSkill(Character c)
		{
			int maxMobCount;
			int maxDamagePerMob; // attack count

			if (nSkillID == 255)
			{
				// TODO figure out if we want validation for this??
				return; // basic attack
			}

			if (nSkillID != 0)
			{
				var pSkill = c.Skills.Get(nSkillID, true);

				if (pSkill is null)
				{
					c.SendMessage("Unable to parse attack. Skill is null.");
					bValidAttack = false;
					return;
				}

				if (pSkill.Template.get_required_combo_count > c.Combat.ComboCounter)
				{
					c.SendMessage($"Combo count insufficient. {pSkill.Template.get_required_combo_count}-{c.Combat.ComboCounter}");
					bValidAttack = false;
					return;
				}

				maxMobCount = pSkill.Template.MobCount(pSkill.nSLV);

				if (pSkill.nSkillID == (int)Skills.KNIGHT_CHARGE_BLOW && c.Skills.Get(Skills.PALADIN_ADVANCED_CHARGE) is SkillEntry pAdvancedCharge)
				{
					maxMobCount = pAdvancedCharge.Template.MobCount(pAdvancedCharge.nSLV);
				}

				if (pSkill.Template.AttackCount(pSkill.nSLV) > 0)
				{
					maxDamagePerMob = pSkill.Template.AttackCount(pSkill.nSLV);
				}
				else if (pSkill.Template.BulletCount > 0)
				{
					maxDamagePerMob = pSkill.Template.BulletCount;
				}
				else
				{
					maxDamagePerMob = 1;
				}

				// lazy check for final attack cuz i cbf to figure out how it really works
				switch (c.Stats.nJob / 100)
				{
					case 1:
					case 11:
					case 3:
					case 13:
						maxDamagePerMob += 1;
						break;
				}
			}
			else if (c.Stats.nJob / 100 == 21)
			{
				maxMobCount = 12; // TODO figure out the real value
				maxDamagePerMob = 1;
			}
			else
			{
				maxMobCount = 1;
				maxDamagePerMob = 1;
			}

			if (maxDamagePerMob == 1 && c.Stats.nJob >= 430 && c.Stats.nJob <= 434)
			{
				maxDamagePerMob = 2;
			}

			if (c.Buffs.Contains((int)Skills.HERMIT_SHADOW_PARTNER) || c.Buffs.Contains((int)Skills.NIGHTWALKER_SHADOW_PARTNER) || c.Buffs.Contains((int)Skills.THIEFMASTER_SHADOW_PARTNER)
			|| c.Buffs.Contains((int)Skills.DUAL4_MIRROR_IMAGING))
			{
				maxDamagePerMob *= 2;
			}

			if (nMobCount > maxMobCount)
			{
				c.SendMessage($"Unable to parse attack. MobCount too high. {nMobCount} > {maxMobCount}.");
				bValidAttack = false;
			}
			else if (nDamagePerMob > maxDamagePerMob)
			{
				c.SendMessage($"Unable to parse attack. AttackCount/BulletCount too high. {nDamagePerMob} > {maxDamagePerMob}.");
				bValidAttack = false;
			}
		}

		private void DecodeAttackInfo(CInPacket p)
		{
			for (int i = 0; i < nMobCount; i++)
			{
				var info = new AttackEntry();

				info.dwMobID = p.Decode4();

				info.nHitAction = p.Decode1();
				info.nForeAction = p.Decode1();//  COutPacket::Encode1(&v468, v376->nForeAction & 0x7F | (v181 << 7));
				info.nFrameIdx = p.Decode1();

				// v218 = CMob::GetCurTemplate(v378->pMob)
				//    && (v166 = CMob::GetTemplate(v378->pMob), v166 != CMob::GetCurTemplate(v378->pMob));
				// v167 = (_BYTE)v218 << 7;

				info.CalcDamageStatIndex = p.Decode1(); // COutPacket::Encode1(&v460, v168 & 0x7F | v167);

				p.Skip(8); // position info

				info.tDelay = p.Decode2();

				for (int j = 0; j < nDamagePerMob; j++)
				{
					info.aDamage[j] = Math.Max(0, p.Decode4());

					//Log.Debug($"[Damaging Mob] dwMobID: {info.dwMobID} aDamage: {info.aDamage[j]}");
				}

				p.Skip(4); // CMob::GetCrc

				aAttackInfo[i] = info;
			}
		}

		public void EncodeAttackInfo(Character c, COutPacket p, byte nSLV, byte nLevel, SendOps nType)
		{
			p.Encode1((byte)(nDamagePerMob | 16 * nMobCount));
			p.Encode1(nLevel); // m_nLevel

			if (nSkillID > 0)
			{
				p.Encode1(nSLV);
				p.Encode4(nSkillID);
			}
			else
			{
				p.Encode1(0);
			}

			//nPassiveSLV = 0;
			//if (nSkillID == (__int16*)3211006
			//  && (v13 = CInPacket::Decode1(v4), v14 = v13, (nPassiveSLV = v13) != 0)
			//  && (v15 = CInPacket::Decode4(v4)) != 0)
			//{
			//    v16 = CSkillInfo::GetSkill(TSingleton < CSkillInfo >::ms_pInstance, v15);
			//    LODWORD(pPassiveSkill) = v16;
			//    if (v16)
			//    {
			//        if (v14 <= 1)
			//            v14 = 1;
			//        v17 = SKILLENTRY::GetMaxLevel(v16);
			//        nPassiveSLV = v14;
			//        if (v14 >= v17)
			//            nPassiveSLV = v17;
			//    }
			//}
			//else
			//{
			//    LODWORD(pPassiveSkill) = 0;
			//}

			if (nSkillID == (int)Skills.SNIPER_STRAFE)
			{
				p.Encode1(0);
				p.Encode4(0);
			}

			p.Encode1(nOption); //bSerialAttack = CInPacket::Decode1(v4) & 0x20;
			p.Encode2(nActionAndDir); //its a short now ???

			//bLeft = ((unsigned int)tByte2 >> 15) & 1;
			//nAction = tByte2 & 0x7FFF;

			if (nAction <= 0x110)
			{
				p.Encode1(nActionSpeed);
				p.Encode1(nMastery);
				p.Encode4(nBulletItemID);

				for (int i = 0; i < nMobCount; i++)
				{
					var info = aAttackInfo[i];

					p.Encode4(info.dwMobID);
					p.Encode1(info.nHitAction); // (byte)(nType != SendOps.LP_UserShootAttack ? 0xFF : 0x07));  //hitAction?

					for (int j = 0; j < nDamagePerMob; j++)
					{
						p.Encode1(info.abCritical[j] != 0);
						p.Encode4(info.aDamage[j]);
					}
				}
			}

			if (nType == SendOps.LP_UserShootAttack)
			{
				p.Encode4(0);
				//ptBallStart.x = (signed __int16)CInPacket::Decode2(v4);
				//ptBallStart.y = (signed __int16)CInPacket::Decode2(v4);
			}

			switch (nSkillID)
			{
				case 2121001:
				case 2221001:
				case 2321001:
				case 22121000:
				case 22151001:
					p.Encode4(tKeyDown);
					break;
				case 33101007:
					{
						if (c.m_dwSwallowMobID <= 0)
							c.m_dwSwallowMobID = 0; // mob dwid, not mob template id

						p.Encode4(c.m_dwSwallowMobID);
						break;
					}
			}
		}
	}
}