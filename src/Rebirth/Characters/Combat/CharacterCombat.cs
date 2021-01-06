using log4net;
using Rebirth.Characters.Modify;
using Rebirth.Characters.Skill;
using Rebirth.Client;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Tools;
using Rebirth.Common.Tools;

namespace Rebirth.Characters.Combat
{
	public class CharacterCombat
	{
		public static ILog Log = LogManager.GetLogger(typeof(CharacterCombat));

		public Character Parent => MasterManager.CharacterPool.Get(dwParentID);
		public int dwParentID { get; set; }

		//-----------------------------------------------------------------------------

		public short ComboCounter { get; set; } // used for both the warrior and aran
		public short nEnergy { get; set; }
		public byte OwlSpiritCount { get; set; }

		public bool bNextAttackCritical { get; set; }

		private DateTime tLastComboCountIncrease { get; set; }

		public bool bVengeanceRequested { get; set; }

		//-----------------------------------------------------------------------------

		public CharacterCombat(int parent)
		{
			dwParentID = parent;

			tLastComboCountIncrease = DateTime.Now;
		}

		public void Dispose()
		{
			// maybe something someday
		}

		//-----------------------------------------------------------------------------

		public static void Handle_UserHit(WvsGameClient c, CInPacket p) => c.Character.Combat.OnHit(c.Character, p);
		public static void Handle_MeleeAttack(WvsGameClient c, CInPacket p) => c.Character.Combat.AttackMob(p, RecvOps.CP_UserMeleeAttack);
		public static void Handle_ShootAttack(WvsGameClient c, CInPacket p) => c.Character.Combat.AttackMob(p, RecvOps.CP_UserShootAttack);
		public static void Handle_MagicAttack(WvsGameClient c, CInPacket p) => c.Character.Combat.AttackMob(p, RecvOps.CP_UserMagicAttack);
		public static void Handle_BodyAttack(WvsGameClient c, CInPacket p) => c.Character.Combat.AttackMob(p, RecvOps.CP_UserBodyAttack);
		public static void Handle_MovingShootAttackPrepare(WvsGameClient c, CInPacket p) => c.Character.Combat.MovingShootAttackPrepare(p);
		public static void Handle_UserAttackUser(WvsGameClient c, CInPacket p) => c.Character.Combat.UserAttackUser(p);
		public static void Handle_RequestIncCombo(WvsGameClient c, CInPacket p) => c.Character.Combat.AranCombo(p);
		public static void Handle_MobAttackMob(WvsGameClient c, CInPacket p) => c.Character.Combat.MobAttackMob(p);
		public static void Handle_UserAttackUser_Specific(WvsGameClient c, CInPacket p)
		{
			//v370 = (CUser*)&apUser[nOrder]->vfptr;
			//COutPacket::COutPacket(&oPacket, 0xDB);
			//LOBYTE(v503) = 25;
			//v177 = CUser::GetCharacterId(v370);
			//COutPacket::Encode4(&oPacket, v177);
			//COutPacket::Encode4(&oPacket, 0xA2u);
			//SendPacket(&oPacket);
			//LOBYTE(v503) = 22;
			//COutPacket::~COutPacket(&oPacket);
		}

		//-----------------------------------------------------------------------------

		// BMS: CUser::OnHit(CUser *this, CInPacket *iPacket)
		// CUserLocal::SetDamaged
		public void OnHit(Character pChar, CInPacket p)
		{
			// if the client is lagging it can send multiple OnHit packets before it realizes it's dead
			if (Parent?.Stats.nHP <= 0) return;

			var tDamagedTime = p.Decode4();
			var nMobAttackIdx = p.Decode1();

			var nMagicElemAttr = p.Decode1(); // Element - 0x00 = elementless, 0x01 = ice, 0x02 = fire, 0x03 = lightning
			var elementalAttack = nMagicElemAttr != 0;

			var nDamageInternal = p.Decode4(); // used for internal calculations
			var nExternalDamageToReflect = nDamageInternal; // broadcast to the client
			var nDamageShownToRemote = nDamageInternal; // also broadcast to the client

			if (nDamageInternal < -1) return; // hecks

			var AAT = (AttackIndexType)nMobAttackIdx;

			var dwObstacleData = 0;
			var dwMobTemplateID = 0;

			CMob mobDamagingPlayer = null;
			CMob mobReflectedOn = null;

			var dwMobID = 0;

			var bLeft = false;
			var nReflect = 0;
			var bGuard = false;
			var nKnockBack = 0;

			var bKnockBack = false;
			var s_pAchilles = false;

			var bPowerGuard = false;
			var tagPOINT = new TagPoint();
			byte nHitAction = 0;

			var nStanceFlag = 0;

			var nMPCon = 0;
			var nEffectSkillID = 0;
			var nMobDamage = 0;
			var nShadowSifterID = 0;

			if (AAT == AttackIndexType.AttackIndex_Counter
				|| AAT == AttackIndexType.AttackIndex_Obstacle
				|| AAT == AttackIndexType.AttackIndex_Stat)
			{
				dwObstacleData = p.Decode2();

				// TODO
				//if (dwObstacleData)
				//{
				//	v53 = dwObstacleData >> 8;
				//	v54 = dwObstacleData;
				//	v55 = CSkillInfo::GetMobSkill(TSingleton < CSkillInfo >::ms_pInstance, dwObstacleData >> 8);
				//	if (v55)
				//	{
				//		v56 = v55->aLevelData.a;
				//		if (v56)
				//			v56 = v56[-1].adwTemplateID.a;
				//		if (v54 <= v56)
				//			CUser::OnStatChangeByMobSkill(&v2->vfptr, v53, v54, &v55->aLevelData.a[v54 - 1], 0, 0);
				//	}
				//}
				//else if (dwMobTemplateID)
				//{
				//	CUser::OnStatChangeByMobAttack(&v2->vfptr, dwMobTemplateID, nMobAttackIdx);
				//}
			}
			else
			{
				// ++v2->m_cheatInspector.m_statistic.nHitCount;

				dwMobTemplateID = p.Decode4();

				var nDamager = p.Decode4();

				mobDamagingPlayer = Parent.Field.Mobs[nDamager];

				if (mobDamagingPlayer is null && (Parent.Field.Template.FieldType != FieldType.BATTLEFIELD /*&& (((MasterManager.MobTemplates[dwMobTemplateID]?.SelfDestructActionType ?? 0) & 4) != 0)*/)) return;

				bLeft = p.Decode1() != 0;
				nReflect = p.Decode1(); // nX is SkillTemplate.nOption
				bGuard = p.Decode1() != 0; // 1 or 0

				//if (bBlocked)
				//    v233 = (bKnockback != 0) + 1;
				//COutPacket::Encode1(&oPacket, v233);
				nKnockBack = p.Decode1();

				bKnockBack = nKnockBack == 2; // this is when a passive/buff stuns the mob

				s_pAchilles = nKnockBack != 0;

				if (bKnockBack || nReflect > 0)
				{
					bPowerGuard = p.Decode1() != 0 && nReflect > 0; // bPowerGuard = CInPacket::Decode1(v6) && v9 ? 1 : 0;
					dwMobID = p.Decode4(); //COutPacket::Encode4(&oPacket, v235); //v235 = CMob::GetMobID(pMob);

					mobReflectedOn = Parent.Field.Mobs[dwMobID];

					if (mobReflectedOn == null) return; // hex

					nHitAction = p.Decode1(); //COutPacket::Encode1(&oPacket, pGuard);

					tagPOINT.Decode(p); // mob pos
										//COutPacket::Encode2(&oPacket, ptHit.x);
										//COutPacket::Encode2(&oPacket, ptHit.y);

					p.Skip(4); // player pos
							   //COutPacket::Encode2(&oPacket, *v236);         //v236 = (v11->vfptr->GetPos)(&v11->vfptr, &result);
							   //COutPacket::Encode2(&oPacket, *(v237 + 4));   //v237 = (v11->vfptr->GetPos)(&v11->vfptr, &ptHit);
				}

				nStanceFlag = p.Decode1();
			}

			if (nDamageInternal > 0)
			{
				{  // TODO
				   // CUser::CheckHitSpeed(&v2->vfptr, tDamagedTime, iPacket, v14);

					// if we implement this we need to make sure we add support for the new stances that were added after bms
					//if (nStanceFlag && !CUser::CheckStanceAble(&v2->vfptr))
					//{
					//	v16 = v2->m_pField;
					//	if (v16)
					//		v17 = v16->m_dwField;
					//	else
					//		v17 = 0;
					//	CVerboseObj::LogError(&v2->vfptr, aIllegalStanceT, v2->m_sCharacterName._m_pStr, v17);
					//}
				}

				//var nMagicGuard = Parent.Buffs.nOptionData(SecondaryStatFlag.MagicGuard);
				// does not modify damage shown to third party, does not show special effect
				if (Parent.Stats.SecondaryStats.nMagicGuard > 0)
				{
					nMPCon = (int)(nDamageInternal * Parent.Stats.SecondaryStats.nMagicGuard * 0.01);

					//if (Parent.Buffs.GetByType(SecondaryStatFlag.Infinity) == null)
					if (Parent.Stats.SecondaryStats.rInfinity <= 0)
					{
						if (Parent.Stats.nMP < nMPCon) nMPCon = Parent.Stats.nMP;
					}

					nDamageInternal -= nMPCon;
				}

				if (mobReflectedOn != null && nReflect > 0 && !mobReflectedOn.Template.Invincible) // handle reflect damage
				{
					if (bPowerGuard)
					{
						// nReflect is useless
						// since it tries to send values north of 255 (max byte)
						// so it overflows and sends some wrong lower number

						//var nPowerGuard = Parent.Buffs.nOptionData(SecondaryStatFlag.PowerGuard);
						var nPowerGuard = Parent.Stats.SecondaryStats.nPowerGuard;

						if (nPowerGuard == 0)
						{
							//nPowerGuard = Parent.Buffs.nOptionData(SecondaryStatFlag.Guard);// maybe switch based on job in the future
							nPowerGuard = Parent.Stats.SecondaryStats.nGuard;
							bPowerGuard = false;
						}

						if (nPowerGuard != 0)
						{
							if (nDamageInternal > Parent.Stats.nHP) nDamageInternal = Parent.Stats.nHP;

							var nReflectDamage = mobReflectedOn.MaxHp * 0.5;

							nMobDamage = (int)(nPowerGuard * nDamageInternal * 0.01);

							if (nMobDamage > nReflectDamage) nMobDamage = (int)nReflectDamage;

							if (mobReflectedOn.Template.Boss) nMobDamage = (int)(nMobDamage * 0.5);

							if (nMobDamage > 0 && mobReflectedOn.Template.FixedDamage > 0)
							{
								nMobDamage = mobReflectedOn.Template.FixedDamage;
							}

							if (!bPowerGuard) // hax
							{
								nReflect = 100;
								nExternalDamageToReflect = nMobDamage;
							}

							//nDamageShownToRemote -= nMobDamage;
							//nDamageInternal -= nMobDamage;

							//if (nDamageShownToRemote < 0) nDamageShownToRemote = 0;
							//if (nDamageInternal < 0) nDamageInternal = 0;
						}
					}
					else if (AAT == AttackIndexType.AttackIndex_Mob_Magic)
					{
						//var nManaReflect = Parent.Buffs.GetByType(SecondaryStatFlag.ManaReflection).Stat[SecondaryStatFlag.ManaReflection];
						//if (nManaReflect.nValue != 0)
						if (Parent.Stats.SecondaryStats.rManaReflection != 0)
						{
							nReflect = 100;
							nExternalDamageToReflect = nMobDamage;

							if (Parent.Stats.nHP > nDamageInternal) nDamageInternal = Parent.Stats.nHP;

							var nMaxDamageR = mobReflectedOn.MaxHp * 0.2;

							//nMobDamage = (int)(nManaReflect.nValue * nDamageInternal * 0.01);
							nMobDamage = Parent.Stats.SecondaryStats.nManaReflection * nDamageInternal / 100;

							if (nMobDamage > nMaxDamageR) nMobDamage = (int)nMaxDamageR;

							//nEffectSkillID = nManaReflect.rValue; // skill id
							nEffectSkillID = Parent.Stats.SecondaryStats.rManaReflection;
						}
					}
				}

				if (s_pAchilles) // i dont think this variable name is correct
				{
					// TODO validate this
					nDamageShownToRemote = -1;
				}

				//var nMesoGuard = Parent.Buffs.nOptionData(SecondaryStatFlag.MesoGuard);
				//if (nMesoGuard > 0 && nDamageInternal >= 2)
				if (Parent.Stats.SecondaryStats.rMesoGuard > 0 && nDamageInternal > 1) // >1 because cant split 1
				{
					var damageReduction = nDamageInternal / 2;
					var mesoReduction = nDamageInternal / 2 * Parent.Stats.SecondaryStats.nMesoGuard / 100;

					if (Parent.Stats.nMoney < mesoReduction)
					{
						damageReduction = 100 * Parent.Stats.nMoney / Parent.Stats.SecondaryStats.nMesoGuard;
						mesoReduction = Parent.Stats.nMoney;
					}

#if DEBUG
					Parent.SendMessage("meso reduction: " + mesoReduction);
#endif

					Parent.Modify.GainMeso(-(int)mesoReduction, false);
					nDamageInternal = (int)(nDamageInternal - damageReduction);
					nDamageShownToRemote = (int)(nDamageShownToRemote - damageReduction);
					nEffectSkillID = (int)Skills.THIEFMASTER_MESO_GUARD;
				}
			}


			if (nDamageInternal < 0)
			{
				var v57 = Parent.Stats.nJob - 112;
				if (v57 > 0)
				{
					var v58 = v57 - 10;
					if (v58 > 0)
					{
						var v59 = v58 - 290;
						if (v59 > 0)
						{
							if (v59 == 10) nShadowSifterID = 4220002;
						}
						else
						{
							nShadowSifterID = 4120002;
						}
					}
					else
					{
						nShadowSifterID = 1220006;
					}
				}
				else
				{
					nShadowSifterID = 1120005;
				}

				if (nShadowSifterID != 0)
				{
					if (Parent.Skills.Get(nShadowSifterID) is null) return; // hecker
				}
				else
				{
					Parent.SendMessage($"Found new shadow sifter ID {nShadowSifterID}. Please take a screenshot and report to staff.");
				}
			}

#if DEBUG
			if (nKnockBack > 0 || nReflect > 0)
			{
				Parent.SendMessage($"KB: {nKnockBack} | dwMobID: {dwMobID} | IncDmg: {nDamageInternal} => {nDamageShownToRemote}");
			}
#endif

			if (bKnockBack) // TODO the rest of the skills that go in here
			{
				if (pChar.Skills.Get(nShadowSifterID) is SkillEntry pKnockBack)
				{
					mobReflectedOn.TryApplySkillDamageStatus(Parent, nShadowSifterID, pKnockBack.nSLV, 0, true);
				}
			}

			switch (JobLogic.JobCodeFromJobID(pChar.Stats.nJob))
			{
				case 1:
					{
						if (pChar.Buffs[(int)Skills.PALADIN_DIVINE_SHIELD] is BuffSkill bs)
						{
							bGuard = true;
							if (--bs.State <= 0)
							{
								pChar.Buffs.Remove(bs);
								pChar.Cooldowns.UpdateOrInsert(bs.nBuffID, (short)bs.Template.Cooltime(bs.nSLV));
							}
						}
						else if (pChar.Skills.Get(Skills.PALADIN_DIVINE_SHIELD) is SkillEntry pDivineShield)
						{
							if (!pChar.Cooldowns.OnCooldown(pDivineShield.nSkillID) && pDivineShield.DoProp())
							{
								pChar.Buffs.AddSkillBuff(pDivineShield.nSkillID, pDivineShield.nSLV, 1.0, (int)pDivineShield.X_Effect);
							}
						}
					}
					break;
				case 3:
					{
						if (pChar.Stats.nJob / 10 == 31)
						{
							if (pChar.Skills.Get((int)Skills.BOWMASTER_VENGEANCE) is SkillEntry bmVeng)
							{
								//mobDamagingPlayer.TryApplySkillDamageStatus
								//	(Parent, bmVeng.nSkillID, bmVeng.nSLV, 0);
								bVengeanceRequested = true;
								Parent.SendPacket(CPacket.UserRequestVengeance());
							}
						}

						if (nDamageShownToRemote <= 0)
						{
							if (pChar.Skills.Get(Skills.RANGER_DODGE) != null || pChar.Skills.Get(Skills.SNIPER_DODGE) != null)
							{
								pChar.m_bNextAttackCritical = true;
							}
						}
					}
					break;
				case 5:
					{
						var piratesRevenge = pChar.Skills.Get((int)Skills.CAPTAIN_COUNTER_ATTACK)
							?? pChar.Skills.Get((int)Skills.VIPER_COUNTER_ATTACK); // lazy implementation

						if (piratesRevenge != null && !pChar.Cooldowns.OnCooldown(piratesRevenge.nSkillID) && piratesRevenge.DoProp())
						{
							pChar.Buffs.AddSkillBuff(piratesRevenge.nSkillID, piratesRevenge.nSLV);
							pChar.Cooldowns.UpdateOrInsert(piratesRevenge.nSkillID, (short)piratesRevenge.X_Effect);
						}
					}
					break;
				case 21:
					{
						if (pChar.Buffs[(int)Skills.ARAN_COMBO_BARRIER] is BuffSkill pCBBuff)
						{
							nDamageInternal = (int)(nDamageInternal * (1 - (pCBBuff.Template.T(pCBBuff.nSLV) * 0.01)));
						}
					}
					break;
				case 22: // evan
					if (pChar.Buffs[(int)Skills.EVAN_MAGIC_SHIELD] is BuffSkill pMSBuff)
					{
						nDamageInternal = (int)(nDamageInternal * (1 - (Parent.Skills.Get(pMSBuff.nBuffID).X_Effect * 0.01)));
						nEffectSkillID = pMSBuff.nSkillID;
					}
					break;
			}

			Parent.Field.Broadcast(CPacket.CUserRemote.UserHit
				(dwParentID, nMobAttackIdx, nExternalDamageToReflect, dwMobTemplateID,
					bLeft, (byte)nReflect, bPowerGuard, dwMobID, nHitAction, tagPOINT, bGuard,
					(byte)nStanceFlag, nShadowSifterID, nDamageShownToRemote),
				pChar);

			if (nDamageInternal > 0)
			{
				Parent.Field.OnUserDamaged(Parent, mobDamagingPlayer, nDamageShownToRemote);

				if (mobReflectedOn != null)
				{
					if (mobReflectedOn.Damage(pChar, nMobDamage, 0))
					{
						Parent.Field.Mobs.Remove(mobReflectedOn);
					}
				}

				if (nEffectSkillID > 0)
				{
					new UserEffectPacket(UserEffect.ShowSkillSpecialEffect)
					{
						nSkillID = nEffectSkillID
					}.BroadcastEffect(Parent);
				}
			}

			pChar.Modify.Heal(-nDamageInternal, -nMPCon);
			Parent.StatisticsTracker.nDamageTaken += nDamageInternal;

			if (pChar.Stats.nHP > 0)
			{
				if (dwObstacleData > 0)
				{
					var nSkillID = dwObstacleData & 0xFF;
					var nSLV = (dwObstacleData >> 8) & 0xFF;

					var template = MasterManager.MobSkillTemplates[nSkillID][nSLV];
					Parent.Buffs.OnStatChangeByMobSkill(template, 0, 0);
				}
				else if (dwMobTemplateID > 0)
				{
					// TODO
				}
			}

			// TODO do exp reduction on death here instead of in Death.Event

			Parent.Buffs.Remove((int)Skills.INFIGHTER_OAK_CASK);

			Parent.Buffs.RemoveFirst(b => b.StatType == SecondaryStatFlag.Morph && b is BuffConsume);
		}

		private void AttackMob(CInPacket p, RecvOps type)
		{
			// WARNING: if returning at any point before the CalcDamage function is called,
			// make sure to call: Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);

			var m_bCurFieldKey = p.Decode1();

			MapleAttack atkInfo;
			switch (type)
			{
				case RecvOps.CP_UserMeleeAttack:
					atkInfo = MapleAttack.ParseMelee(p, Parent);
					break;
				case RecvOps.CP_UserShootAttack:
					atkInfo = MapleAttack.ParseShoot(p, Parent);
					break;
				case RecvOps.CP_UserMagicAttack:
					atkInfo = MapleAttack.ParseMagic(p, Parent);
					break;
				case RecvOps.CP_UserBodyAttack:
					atkInfo = MapleAttack.ParseBody(p, Parent);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type));
			}

			if (!atkInfo.bValidAttack)
			{
				Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
				return;
			}

			var pSkill = Parent.Skills.Get(atkInfo.nSkillID, true);

			var nWeaponItemID = InventoryManipulator.GetItem(Parent, BodyPart.BP_WEAPON, false)?.nItemID ?? 0;

			// handle shooting skill item consumption
			if (!atkInfo.bNoItemConsume)
			{
				var nWT = ItemConstants.get_weapon_type(nWeaponItemID);

				var nBulletItemID = Parent.InventoryConsume.Get(atkInfo.nBulletItemPos);

				if (nBulletItemID == null)
				{
					Parent.SendMessage($"Invalid bullet pos. {atkInfo.nBulletItemPos}");
					Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
					return; // PE
				}

				atkInfo.nBulletItemID = nBulletItemID.nItemID; // assign this first so that itll get overriden by the cash item if it exists

				switch (nWT) // todo enum as usual
				{
					case 45: // bow
						atkInfo.nBulletCashItemPos = 0;
						if (!ItemConstants.IsBowArrow(nBulletItemID.nItemID))
						{
							Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
							return; // PE
						}
						break;
					case 46: // xbow
						atkInfo.nBulletCashItemPos = 0;
						if (!ItemConstants.IsXBowArrow(nBulletItemID.nItemID))
						{
							Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
							return; // PE
						}
						break;
					case 47: // throwing glove

						if (atkInfo.nBulletCashItemPos > 0)
						{
							if (Parent.InventoryCash.Get(atkInfo.nBulletCashItemPos) == null)
							{
								Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
								return; // PE
							}

							atkInfo.nBulletItemID = Parent.InventoryCash.Get(atkInfo.nBulletCashItemPos).nItemID;
						}

						if (!ItemConstants.IsThrowingStar(nBulletItemID.nItemID))
						{
							Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
							return; // PE
						}
						break;
					case 49: // gun
						atkInfo.nBulletCashItemPos = 0;
						if (!ItemConstants.IsBullet(nBulletItemID.nItemID))
						{
							Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
							return; // PE
						}
						break;
					case 0: // no weapon equipped -- PE
						Parent.SendMessage("Unable to find equipped weapon.");
						Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
						return;
					default:
						Parent.SendMessage($"Unknown error. WID: {nWeaponItemID} - WType: {nWT} - Slot: {-(short)BodyPart.BP_WEAPON}.");
						Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
						return;
				}

				if (nBulletItemID.nNumber == 0) // fuck checking for a second item, if they have > 0 in the slot the attack will work regardless of bullet count
				{
					Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
					return;
				}

				int nItemConsumeCount;

				if (pSkill == null)
				{
					nItemConsumeCount = 1;
				}
				else if (pSkill.Template.BulletCount > 0)
				{
					nItemConsumeCount = pSkill.Template.BulletCount;
				}
				else if (pSkill.Template.BulletConsume > 0)
				{
					nItemConsumeCount = pSkill.Template.BulletConsume;
				}
				//else if (pSkill.Template.MobCount(pSkill.nSLV) > 0)
				//{
				//	nItemConsumeCount = pSkill.Template.MobCount(pSkill.nSLV);
				//}
				//else if (pSkill.Template.AttackCount(pSkill.nSLV) > 0)
				//{
				//	nItemConsumeCount = pSkill.Template.AttackCount(pSkill.nSLV);
				//}
				else
				{
					nItemConsumeCount = 1; // atkInfo.nMobCount * atkInfo.nDamagePerMob; // ??
				}

				//if (Parent.Buffs.GetByType(SecondaryStatFlag.ShadowPartner) != null)
				if (Parent.Stats.SecondaryStats.rShadowPartner != 0)
				{
					nItemConsumeCount *= 2;
				}

				nBulletItemID.nNumber = (short)Math.Max(0, nBulletItemID.nNumber - nItemConsumeCount);
				Parent.Modify.Inventory(ctx => ctx.UpdateQuantity(InventoryType.Consume, atkInfo.nBulletItemPos, nBulletItemID.nNumber));
			}

			var field = Parent.Field;
			var nLevel = Parent.Stats.nLevel;
			var nSLV = pSkill?.nSLV ?? 0;

			if (nSLV > 0)
			{
				// todo verify skills requiring a mount
				switch ((Skills)atkInfo.nSkillID)
				{
					case Skills.BOWMASTER_VENGEANCE:
						if (!bVengeanceRequested) return;
						bVengeanceRequested = false;
						break;
					case Skills.MECHANIC_SG88:
						{
							if (Parent.nPreparedSkill != atkInfo.nSkillID)
							{
								Parent.Action.Enable();
								Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
								return;
							}
							Parent.nPreparedSkill = 0;
						}
						goto default;
					case Skills.MECHANIC_SIEGE2:
					case Skills.MECHANIC_SIEGE1:
						// these skills have already been cast
						break;
					case Skills.ARAN_BODY_PRESSURE when !Parent.Buffs.Contains(atkInfo.nSkillID): // body attack skills 
						Parent.Action.Enable();
						return;
					case Skills.NIGHTWALKER_POISON_BOMB:
						if (!Parent.Skills.Cast(atkInfo.nSkillID, false, true))
						{
							Parent.Action.Enable();
							Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
							return;
						}
						CGrenade.CastGrenadeSkill(Parent, atkInfo);
						break;
					case Skills.EVAN_KILLING_WING:
					case Skills.VALKYRIE_HOMING:
					case Skills.CAPTAIN_ADVANCED_HOMING:
						if (!Parent.Skills.Cast(atkInfo.nSkillID, false, true))
						{
							Parent.Action.Enable();
							Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
							return;
						}
						break;
					case Skills.MECHANIC_FLAMETHROWER:
					case Skills.MECHANIC_FLAMETHROWER_UP:
						if (!Parent.Skills.Cast(atkInfo.nSkillID, false, true))
						{
							Parent.Action.Enable();
							Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
							return;
						}
						var newBuff = new BuffSkill(atkInfo.nSkillID, nSLV);
						newBuff.Generate();
						Parent.Buffs.AddItemBuff(newBuff);
						break;
					case Skills.BMAGE_TELEPORT_MASTERY:
					case Skills.MAGE1_TELEPORT_MASTERY:
					case Skills.MAGE2_TELEPORT_MASTERY:
					case Skills.PRIEST_TELEPORT_MASTERY:
						if (!Parent.Buffs.Contains(atkInfo.nSkillID))
						{
							Parent.Action.Enable();
							Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
							return;
						}
						break;
					default:
						if (!Parent.Skills.Cast(atkInfo.nSkillID, false))
						{
							Parent.Action.Enable();
							Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
							return;
						}
						break;
				}
			}

			Parent.Buffs.Remove(Parent.Stats.SecondaryStats.rDarkSight);

			if (atkInfo.bNextShootExJablin)
			{
				if (Parent.Stats.nJob != 412)
				{
					Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
					return;
				}
			}
			else
			{
				if (Parent.Stats.nJob == 412 && (atkInfo.nOption & 0x40) == 0) // check job and spirit javelin buff (shadow stars)
				{
					var pExpertJavelin = Parent.Skills.Get((int)Skills.NIGHTLORD_EXPERT_JAVELIN);

					if (pExpertJavelin?.DoProp() ?? false)
					{
						Parent.SendPacket(new COutPacket(SendOps.LP_UserRequestExJablin));
						Parent.m_bNextAttackCritical = true;
					}
				}
			}

			switch (Parent.Stats.nJob / 10)
			{
				case 11:
					{
						// validation has already occured
						if (atkInfo.nSkillID == (int)Skills.HERO_ENRAGE)
						{
							ComboCounter -= 10;
						}

						if (atkInfo.nMobCount > 0)
						{
							ProcessComboAttack((int)Skills.CRUSADER_COMBO_ATTACK,
								(int)Skills.HERO_ADVANCED_COMBO);
						}
					}
					break;
				case 12:
					{
						if ((Skills)atkInfo.nSkillID == Skills.KNIGHT_CHARGE_BLOW)
						{
							if (!Parent.Buffs.Remove(Parent.Stats.SecondaryStats.rWeaponCharge))
							{
								Parent.CalcDamage.m_RndGenForCharacter.Skip(7 * atkInfo.nMobCount);
								return;
							}
						}
					}
					break;
				case 111:
					if (atkInfo.nMobCount > 0)
					{
						ProcessComboAttack((int)Skills.SOULMASTER_COMBO_ATTACK,
							(int)Skills.SOULMASTER_ADVANCED_COMBO);
					}
					break;
				case 321:
					{
						if (Parent.Buffs[(int)Skills.BMAGE_REVIVE] is BuffSkill bs &&
							Parent.Skills.Get(bs.nBuffID).DoProp())
						{
							Parent.Field.Summons.CreateSummon(Parent, bs.nSkillID, bs.nSLV, Parent.Position.X,
								Parent.Position.Y);
						}
					}
					break;
			}

			var deadMobs = new List<CMob>();
			var firstMobDead = false;

			var pPickpocketBuff = Parent.Buffs.GetOrDefault((int)Skills.THIEFMASTER_PICKPOCKET);

			var nTotalDmg = 0;
			var nTotalAttacks = 0;

			for (var i = 0; i < atkInfo.nMobCount; i++)
			{
				var info = atkInfo.aAttackInfo[i];
				var dwMobId = info.dwMobID;

				var target = field.Mobs[dwMobId];

				if (target is null)
				{
					Parent.CalcDamage.m_RndGenForCharacter.Skip();
					continue;
				}

				var nDamage = 0;

				switch (type)
				{
					case RecvOps.CP_UserMagicAttack:
						{
							Parent.CalcDamage.MDamage(Parent, target, pSkill, atkInfo, nWeaponItemID,
								out var aDamage,
								out var abCritical
							);

							for (var j = 0; j < atkInfo.nDamagePerMob; j++)
							{
								if (info.aDamage[j] * 1.1 < aDamage[j])
								{
									Parent.SendMessage(
										$"Damage calc failed. Incoming: {info.aDamage[j]} vs Calculated: {aDamage[j]}. Please report to staff with a screenshot.");
								}

								info.abCritical[j] = abCritical[j];
							}
						}
						break;
					default:
						{
							Parent.CalcDamage.PDamage(Parent, target, pSkill, atkInfo, nWeaponItemID,
								out var aDamage,
								out var abCritical
							);
						}
						break;
				}

				for (var j = 0; j < atkInfo.nDamagePerMob; j++)
				{
					nDamage += info.aDamage[j];
				}

				nTotalAttacks += atkInfo.nDamagePerMob;

				if (nDamage <= 0) continue;

				nTotalDmg += nDamage;

				// damage target, return true if dead
				if (target.Damage(Parent, nDamage, info.tDelay))
				{
					if (info == atkInfo.aAttackInfo[0])
					{
						firstMobDead = true;
					}

					deadMobs.Add(target);
					continue;
				}

				// handle effects from buffs
				switch (Parent.Stats.nJob / 10)
				{
					case 12:
						TryApplyMobStatusFromBuff(target, (int)Skills.KNIGHT_CHARGE_BLOW, nDamage);
						break;
					case 21:
					case 22:
					case 23:
						TryDoingManaEater(target, Parent.Stats.nJob * 10000);
						break;
					case 31:
						TryApplyMobStatusFromBuff(target, (int)Skills.BOWMASTER_HAMSTRING, nDamage);
						break;
					case 32:
						TryApplyMobStatusFromBuff(target, (int)Skills.CROSSBOWMASTER_BLIND, nDamage);
						break;
					case 42:
						{
							if (pPickpocketBuff is BuffSkill bs)
							{
								target.Stats.PickPocketRate = Math.Max(target.Stats.PickPocketRate, (int)((bs.Stat[SecondaryStatFlag.PickPocket].nValue + 100) * 0.01));
								TryApplyMobStatusFromBuff(target, (int)Skills.THIEFMASTER_PICKPOCKET, nDamage);
							}
						}
						break;
					case 211:
						TryApplyMobStatusFromBuff(target, (int)Skills.ARAN_SNOW_CHARGE, nDamage);
						break;
					case 221:
						TryApplyMobStatusFromBuff(target, (int)Skills.EVAN_SLOW, nDamage);
						Parent.Buffs.Remove((int)Skills.EVAN_SLOW);
						break;
					case 321:
						break;
					case 331:
						{
							TryApplyMobStatusFromBuff(target, (int)Skills.WILDHUNTER_BLIND, nDamage);

							if (atkInfo.nSkillID == (int)Skills.WILDHUNTER_NERVEGAS)
							{
								TryApplyMobStatusFromSkill(target, (int)Skills.WILDHUNTER_NERVEGAS, nDamage);
							}
						}
						break;
				}

				// apply skill statuses if not dead
				if (atkInfo.nSkillID != 0)
				{
					TryApplyMobStatusFromSkill(target, atkInfo.nSkillID, nDamage);
				}

				switch (Parent.Stats.nJob)
				{
					case 412:
						TryApplyVenom(target, (int)Skills.NIGHTLORD_VENOM);
						break;
					case 422:
						TryApplyVenom(target, (int)Skills.SHADOWER_VENOM);
						break;
					case 434:
						TryApplyVenom(target, (int)Skills.DUAL5_VENOM);
						break;
					case 1411:
						TryApplyVenom(target, (int)Skills.NIGHTWALKER_VENOM);
						break;
				}
			}

			if (nTotalDmg > 0)
			{
				Parent.DamageTracker.ApplyAttack(nTotalAttacks, nTotalDmg);
				Parent.StatisticsTracker.nDamageDealt += nTotalDmg;

				TryDoHealingAttackFromBuff((int)Skills.BMAGE_BLOOD_DRAIN, nTotalDmg);
				TryDoHealingAttackFromBuff((int)Skills.ARAN_COMBO_DRAIN, nTotalDmg);

				if (atkInfo.nSkillID != 0)
				{
					switch ((Skills)atkInfo.nSkillID)
					{
						case Skills.NIGHTWALKER_VAMPIRE:
						case Skills.STRIKER_ENERGY_DRAIN:
						case Skills.BUCCANEER_ENERGY_DRAIN:
						case Skills.ASSASSIN_DRAIN:
						case Skills.WILDHUNTER_CLAW_CUT:
							{
								Parent.Modify.Heal((int)(nTotalDmg * pSkill.Template.X(nSLV) * 0.01));
							}
							break;
					}

					if (Parent.Skills.Get((int)Skills.DRAGONKNIGHT_DRAGON_JUDGEMENT) is SkillEntry dragonWisdom)
					{
						switch ((Skills)atkInfo.nSkillID)
						{
							case Skills.DRAGONKNIGHT_DRAGON_ROAR:
							case Skills.DRAGONKNIGHT_SACRIFICE:
							case Skills.DRAGONKNIGHT_DRAGON_BURSTER:
							case Skills.DRAGONKNIGHT_DRAGON_THRESHER:
								{
									if (dragonWisdom.DoProp())
									{
										Parent.Modify.Heal((int)(nTotalDmg * pSkill.Template.X(nSLV) * 0.01));
									}
								}
								break;
						}
					}
				}
			}

			COutPacket atkPacket;

			switch (type)
			{
				case RecvOps.CP_UserMeleeAttack:
					atkPacket = CPacket.CUserRemote.UserMeleeAttack(Parent.dwId, atkInfo, nSLV, nLevel);

					if (atkInfo.nMobCount > 0)
					{
						switch (Parent.Stats.nJob / 10)
						{
							case 43 when OwlSpiritCount > 0:
								{
									if (OwlSpiritCount <= 1) Parent.Buffs.Remove((int)Skills.DUAL4_OWL_DEATH);
									OwlSpiritCount -= 1;
								}
								break;
							case 51:
								ProcessEnergySkill((int)Skills.BUCCANEER_ENERGY_CHARGE, atkInfo.nMobCount);
								break;
							case 151:
								ProcessEnergySkill((int)Skills.STRIKER_ENERGY_CHARGE, atkInfo.nMobCount);
								break;
						}
					}
					break;
				case RecvOps.CP_UserMagicAttack:
					atkPacket = CPacket.CUserRemote.UserMagicAttack(Parent.dwId, atkInfo, nSLV, nLevel);
					break;
				case RecvOps.CP_UserShootAttack:
					atkPacket = CPacket.CUserRemote.UserShootAttack(Parent, atkInfo, nSLV, nLevel);
					if (SkillLogic.is_guided_bullet_skill(atkInfo.nSkillID) && atkInfo.nMobCount > 0 && !firstMobDead && atkInfo.aAttackInfo[0].aDamage[0] > 0)
					{
						Parent.Buffs.Remove(atkInfo.nSkillID);

						var curGbCharId = field.Mobs[atkInfo.aAttackInfo[0].dwMobID].dwGuidedBulletTargetCharID;

						if (curGbCharId != dwParentID && field.Users[curGbCharId] is Character c)
						{
							c.Buffs.Remove((int)Skills.CAPTAIN_ADVANCED_HOMING);
							c.Buffs.Remove((int)Skills.VALKYRIE_HOMING);
							c.Buffs.Remove((int)Skills.EVAN_KILLING_WING);
						}

						field.Mobs[atkInfo.aAttackInfo[0].dwMobID].dwGuidedBulletTargetCharID = dwParentID;

						var newBuff = new BuffSkill(atkInfo.nSkillID, nSLV);
						newBuff.dwMobId = atkInfo.aAttackInfo[0].dwMobID;

						//Parent.SendMessage("Mob ID: " + newBuff.dwMobId);
						newBuff.Generate();

						Parent.Buffs.Add(newBuff);
					}
					break;
				case RecvOps.CP_UserBodyAttack:
					atkPacket = CPacket.CUserRemote.UserBodyAttack(Parent.dwId, atkInfo, nSLV, nLevel);
					break;
				default:
					throw new InvalidOperationException($"Unhandled OpCode [{Enum.GetName(type.GetType(), type)}] passed to CharacterCombat.Attack().");
			}

			field.Broadcast(atkPacket, Parent);

			// exp is handled in mob removal
			foreach (var mob in deadMobs)
			{
				field.Mobs.Remove(mob.dwId);
			}
		}

		public int TryDoingManaEater(CMob mob, int nSkillID)
		{
			if (Parent.Skills.Get(nSkillID) is SkillEntry bs)
			{
				if (!bs.DoProp()) return 0;

				if (Parent.Stats.nHP > 0 && !mob.Template.Boss)
				{
					var mpToRemove = Math.Min(mob.Stats.MP, (int)(mob.MaxMp * bs.X_Effect / 100.0));

					mob.Heal(0, -mpToRemove);

					Parent.Modify.Heal(0, mpToRemove);

					var effect = new UserEffectPacket(UserEffect.SkillUse)
					{
						nSkillID = nSkillID,
						nSLV = bs.nSLV
					};

					effect.BroadcastEffect(Parent);

					return mpToRemove;
				}
			}
			return 0;
		}

		private void TryDoHealingAttackFromBuff(int nSkillID, int nDamage)
		{
			if (Parent.Buffs[nSkillID] is BuffSkill bs)
			{
				var maxHealAmount = Parent.BasicStats.nMHP * 0.1;
				var baseHealAmount = nDamage * (bs.Stat[SecondaryStatFlag.ComboDrain].nValue + 1) * 0.01;

				if (baseHealAmount > maxHealAmount)
					baseHealAmount = maxHealAmount;

				Parent.Modify.Heal((int)baseHealAmount);
			}
		}

		private void TryApplyMobStatusFromBuff(CMob mob, int nSkillID, int nDamage)
		{
			if (Parent.Buffs[nSkillID] is BuffSkill bs)
			{
				mob.TryApplySkillDamageStatus(Parent, bs.nSkillID, bs.nSLV, 0);
			}
		}

		private void TryApplyVenom(CMob mob, int nSkillID) // CMob::OnMobStatChangeSkill
		{
			if (Parent.Skills.Get(nSkillID) is SkillEntry se)
			{
				if (!se.DoProp()) return;
				// can be applied to bosses

				var stat = Parent.BasicStats.nSTR + Parent.BasicStats.nLUK;
				var statbonus = (stat * 0.8) + Constants.Rand.Next() % stat;
				var nOption = se.Template.Dot(se.nSLV) * (Parent.BasicStats.nDEX + 5 * statbonus) / 49;
				if (nOption > short.MaxValue) nOption = short.MaxValue; // idk if this is possible but we dont want negative dmg

				mob.Stats.TempStats.RegisterMobStats(dwParentID, 0, new MobStatEntry(MobStatType.Venom, nSkillID, (short)nOption, 3));
			}
		}

		private void TryApplyMobStatusFromSkill(CMob mob, int nSkillID, int nDamage)
		{
			if (Parent.Skills.Get(nSkillID) is SkillEntry se)
			{
				mob.TryApplySkillDamageStatus(Parent, se.nSkillID, se.nSLV, 0);
			}
		}

		private void MovingShootAttackPrepare(CInPacket p)
		{
			// CUserLocal::TryDoingSmoothingMovingShootAttackPrepare

			//COutPacket::COutPacket(&oPacket, v54);
			//LOBYTE(v71) = 3;
			//COutPacket::Encode4(&oPacket, nSkillID);
			//COutPacket::Encode2(&oPacket, ((_WORD)v35 << 15) | v20 & 0x7FFF);
			//COutPacket::Encode1(&oPacket, v45);
			//CClientSocket::SendPacket(TSingleton < CClientSocket >::ms_pInstance, &oPacket);

			var nSkillID = p.Decode4();
			var nMoveAction = p.Decode2(); // bLeft?? maybe
			var nActionSpeed = p.Decode1();

			var pSkill = Parent.Skills.Get(nSkillID);

			if (pSkill is null) return;

			// todo is there any more handling that needs to be done??

			Parent.Field.Broadcast(
				CPacket.CUserRemote.UserMovingShootAttackPrepare(Parent.dwId, Parent.Stats.nLevel, pSkill.nSLV, pSkill.nSkillID, nMoveAction, nActionSpeed),
				Parent);
		}

		private void UserAttackUser(CInPacket p)
		{
			// todo pvp
		}

		private void AranCombo(CInPacket p)
		{
			if (ComboCounter > 0 && tLastComboCountIncrease.SecondsSinceStart() > 8)
			{
				ComboCounter = 0;
			}

			tLastComboCountIncrease = DateTime.Now;

			if (Parent.Skills.Get((int)Skills.ARAN_COMBO_ABILITY) is SkillEntry se)
			{
				switch (Parent.Stats.nJob)
				{
					case 2100:
					case 2110:
					case 2111:
					case 2112:
						if (ComboCounter < 30_000) //ARAN COMBO LIMIT
						{
							ComboCounter += 1;
						}

						var buff = ComboCounter > 100 ? 100 : ComboCounter;

						if (buff == 1 || buff % 10 == 0)
						{
							Parent.Buffs.AddSkillBuff(se.nSkillID, se.nSLV, 1, buff);
						}

						Parent.SendPacket(CPacket.IncCombo(ComboCounter)); //m_nCombo
						break;
				}
			}
		}

		private void MobAttackMob(CInPacket p)
		{
			//CMob::SetDamagedByMob
			// Recv [CP_MobAttackMob] [E9 00] [1D 27 00 00] [75 04 00 00] [11 27 00 00] [FF] [E1 00 00 00] [00] [09 FF] [F8 00]

			//COutPacket::COutPacket((COutPacket*)&pTemplate, 0xE9);
			//rcBody.bottom = 3;
			//v40 = CMob::GetMobID((CMob*)nDamage);
			//COutPacket::Encode4((COutPacket*)&pTemplate, v40);
			//COutPacket::Encode4(
			//  (COutPacket*)&pTemplate,
			//  *((_DWORD*)TSingleton < CWvsContext >::ms_pInstance._m_pStr + 2093));
			//v41 = CMob::GetMobID(v7);
			//COutPacket::Encode4((COutPacket*)&pTemplate, v41);
			//COutPacket::Encode1((COutPacket*)&pTemplate, vx);
			//COutPacket::Encode4((COutPacket*)&pTemplate, v23);
			//COutPacket::Encode1((COutPacket*)&pTemplate, vy < 0);
			//COutPacket::Encode2((COutPacket*)&pTemplate, v38);
			//COutPacket::Encode2((COutPacket*)&pTemplate, v39);

			var dwAttackerMobID = p.Decode4();
			var nTemplateID = p.Decode4();
			var dwTargetMobID = p.Decode4();

			var nAttackIdx = p.Decode1();

			var nDamage = p.Decode4();

			var bLeft = p.Decode1();

			var nX = p.Decode2(); // v38 = (rcBody.right + rcBody.left) / 2;
			var nY = p.Decode2(); // v39 = (rcBody.top + rcBody.bottom) / 2;

			var pAttackerMob = Parent.Field.Mobs.GetOrDefault(dwAttackerMobID);
			var pTargetMob = Parent.Field.Mobs.GetOrDefault(dwTargetMobID);

			if (pAttackerMob is null || pTargetMob is null) return;

			var cts = pAttackerMob.Stats.TempStats.GetOrDefault((int)MobStatType.Dazzle);

			if (cts is null || cts.nOption != dwParentID) return;

			Parent.Field.Broadcast(
				CPacket.CMobPool.MobAttackedByMob(dwTargetMobID, nAttackIdx, nDamage, pTargetMob.nMobTemplateId, bLeft),
				Parent); // exclude

			if (pTargetMob.Damage(Parent, nDamage, 0))
			{
				Parent.Field.Mobs.Remove(pTargetMob);
			}
		}

		//-----------------------------------------------------------------------------

		private void ProcessComboAttack(int nComboAttackId, int nAdvancedComboId)
		{
			// This handles the combo attack ability for KoC warrior and adventurer warrior.
			//if (!Parent.Buffs.Contains(nComboAttackId)) return;

			//var cBuff = (BuffSkill)Parent.Buffs.GetOrDefault(nComboAttackId);

			if (Parent.Buffs[nComboAttackId] is BuffSkill pComboAttack)
			{
				var nMaxComboCount = pComboAttack.Template.X(pComboAttack.nSLV) + 1; // add 2 because (state 6) = (5 orbs)

				//Parent.SendMessage("Max combo count: " + nMaxComboCount);

				var bDoubleCharge = false;

				if (nMaxComboCount > 5 && Parent.Skills.Contains(nAdvancedComboId))
				{
					nMaxComboCount = (int)((Parent.Skills.Get(nAdvancedComboId)?.X_Effect + 1) ?? nMaxComboCount);
					bDoubleCharge = Parent.Skills.Get(nAdvancedComboId)?.DoProp() ?? false;
				}

				if (nMaxComboCount > ComboCounter)
				{
					if (ComboCounter < 1) ComboCounter += 1;

					ComboCounter += 1;

					if (bDoubleCharge && nMaxComboCount > ComboCounter)
					{
						ComboCounter += 1;
					}

					pComboAttack.State = ComboCounter;
					Parent.Buffs.UpdateBuffInfo(pComboAttack);
					//Parent.SendMessage("Combo Counter: " + cBuff.State);
				}
			}
		}

		private void ProcessEnergySkill(int nEnergySkillId, byte nMobCount)
		{
			// Handles Pirate energy skill increase 
			var energySkill = Parent.Skills.Get(nEnergySkillId);

			if (energySkill is null) return;

			nEnergy = (short)Math.Min((energySkill.X_Effect * nMobCount) + nEnergy, SkillLogic.EnergyMax);

			var cBuff = Parent.Buffs.GetOrDefault(nEnergySkillId);

			if (cBuff is null)
			{
				cBuff = new BuffSkill(energySkill.nSkillID, energySkill.nSLV);
				cBuff.Generate(1);
			}

			cBuff.State = nEnergy;

			Parent.Buffs.UpdateBuffInfo(cBuff);
		}
	}
}