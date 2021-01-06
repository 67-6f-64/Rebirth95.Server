using Rebirth.Entities;
using Rebirth.Network;
using System;
using Rebirth.Characters.Stat;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Tools.Formulas;

namespace Rebirth.Characters.Modify
{
	public sealed class StatModifier
	{
		private readonly CharacterStat _stat;

		public ModifyStatFlags Flag { get; private set; } = 0;

		public byte Skin
		{
			get => _stat.nSkin;
			set
			{
				Flag |= ModifyStatFlags.Skin;
				_stat.nSkin = value;
			}
		}

		public int Face
		{
			get => _stat.nFace;
			set
			{
				Flag |= ModifyStatFlags.Face;
				_stat.nFace = value;
			}
		}

		public int Hair
		{
			get => _stat.nHair;
			set
			{
				Flag |= ModifyStatFlags.Hair;
				_stat.nHair = value;
			}
		}

		// TODO: Pet stuff

		public byte Level
		{
			get => _stat.nLevel;
			set
			{
				Flag |= ModifyStatFlags.Level;
				_stat.nLevel = value;
			}
		}

		public short Job
		{
			get => _stat.nJob;
			set
			{
				Flag |= ModifyStatFlags.Job;
				_stat.nJob = value;
			}
		}

		public short SubJob
		{
			get => _stat.nSubJob;
			set => _stat.nSubJob = value; // i guess theres no flag for this??
		}

		public short STR
		{
			get => _stat.nSTR;
			set
			{
				Flag |= ModifyStatFlags.STR;
				_stat.nSTR = value;
			}
		}

		public short DEX
		{
			get => _stat.nDEX;
			set
			{
				Flag |= ModifyStatFlags.DEX;
				_stat.nDEX = value;
			}
		}

		public short INT
		{
			get => _stat.nINT;
			set
			{
				Flag |= ModifyStatFlags.INT;
				_stat.nINT = value;
			}
		}

		public short LUK
		{
			get => _stat.nLUK;
			set
			{
				Flag |= ModifyStatFlags.LUK;
				_stat.nLUK = value;
			}
		}

		public int MHP
		{
			get => _stat.nMHP;
			set
			{
				Flag |= ModifyStatFlags.MaxHP;
				_stat.nMHP = value;
			}
		}
		public int MMP
		{
			get => _stat.nMMP;
			set
			{
				Flag |= ModifyStatFlags.MaxMP;
				_stat.nMMP = value;
			}
		}

		public int HP
		{
			get => _stat.nHP;
			set
			{
				Flag |= ModifyStatFlags.HP;
				_stat.nHP = value;
			}
		}

		public int MP
		{
			get => _stat.nMP;
			set
			{
				Flag |= ModifyStatFlags.MP;
				_stat.nMP = value;
			}
		}

		public short AP
		{
			get => _stat.nAP;
			set
			{
				Flag |= ModifyStatFlags.AP;
				_stat.nAP = value;
			}
		}

		/// <summary>
		/// This is necessary because extendedSP jobs will use a different index based on their job tier
		/// </summary>
		public void ReduceSpBySkillID(int nSkillID)
		{
			Flag |= ModifyStatFlags.SP;
			_stat.extendSP[JobLogic.GetExtendedSPIndexBySkill(nSkillID)] -= 1;
		}

		// moved to GW_CharacterStat for easier access outside of this class
		//public int GetSpBySkillID(int nSkillID)
		//{
		//	return _stat.extendSP[JobLogic.GetExtendedSPIndexBySkill(nSkillID)];
		//}

		/// <summary>
		/// This will get/modify SP in the current jobs extendedSP index
		/// </summary>
		public short SP
		{
			get => (short)_stat.extendSP[JobLogic.GetExtendedSPIndexByJob(Job)];
			set
			{
				Flag |= ModifyStatFlags.SP;
				_stat.extendSP[JobLogic.GetExtendedSPIndexByJob(Job)] = value;
			}
		}

		public int EXP
		{
			get => _stat.nEXP;
			set
			{
				Flag |= ModifyStatFlags.EXP;
				_stat.nEXP = value;
			}
		}

		public short POP
		{
			get => _stat.nPOP;
			set
			{
				Flag |= ModifyStatFlags.POP;
				_stat.nPOP = value;
			}
		}

		public int Money
		{
			get => _stat.nMoney;
			set
			{
				Flag |= ModifyStatFlags.Money;
				_stat.nMoney = value;
			}
		}

		/// <summary>
		/// Assumes that the pet locker items have already been assigned.
		/// </summary>
		public void UpdatePetLocker()
		{
			Flag |= ModifyStatFlags.Pet;
			Flag |= ModifyStatFlags.Pet2;
			Flag |= ModifyStatFlags.Pet3;
		}

		private Character _parent;

		public StatModifier(Character character)
		{
			_parent = character;
			_stat = _parent.Stats;
		}

		/// <summary>
		/// Should be used for exp loss on death / other exp penalties.
		/// Uses BMS formula as a base.
		/// </summary>
		public void decrease_exp(bool bTown)
		{
			if (EXP <= 0 || Level >= Constants.MaxCharLevel)
				return;

			double reductionRate;

			if (bTown)
			{
				reductionRate = 0.01;
			}
			else
			{
				if (Job / 100 == 3)
				{
					reductionRate = 0.08;
				}
				else
				{
					reductionRate = 0.2;
				}

				reductionRate /= _parent.BasicStats.nLUK + 0.05;
			}

			var prevExp = EXP;

			EXP = Math.Max(0, EXP - (int)(NextLevel.get_next_level_exp(Level) * reductionRate));

			_parent.StatisticsTracker.nExpLostOnDeath += prevExp - EXP;
		}

		/// <summary>
		/// Uses BMS formula as a base.
		/// </summary>
		public bool try_process_levelup()
		{
			if (Level >= Constants.MaxCharLevel)
			{
				EXP = 0;
				Level = Constants.MaxCharLevel;
				return false;
			}

			// make sure level increase is first cuz the rest are dependant on level
			Level += 1;

			SP += (short)GetSpGainOnLevel();
			AP += (short)GetApGainOnLevel();

			SetHpMpStatGainFromLevel();

			return true;
		}

		private void SetHpMpStatGainFromLevel()
		{
			if (JobLogic.is_beginner_job(Job))
			{
				MHP += Constants.Rand.Next(12, 16);
				MMP += Constants.Rand.Next(10, 12);
			}
			else
			{
				switch (Math.Floor(Job * 0.01))
				{
					case 1:
					case 11:
					case 21:
						MHP += Constants.Rand.Next(48, 52);
						MMP += Constants.Rand.Next(4, 6);
						break;
					case 2:
					case 12:
						MHP += Constants.Rand.Next(10, 14);
						MMP += Constants.Rand.Next(48, 52);
						break;
					case 3:
					case 13:
					case 33:
					case 4:
					case 14:
						MHP += Constants.Rand.Next(20, 24);
						MMP += Constants.Rand.Next(14, 16);
						break;
					case 5:
					case 15:
					case 35:
						MHP += Constants.Rand.Next(37, 41);
						MMP += Constants.Rand.Next(18, 22);
						break;
					case 20:
						MHP += Constants.Rand.Next(50, 52);
						MMP += Constants.Rand.Next(4, 6);
						break;
					case 22:
						MHP += Constants.Rand.Next(12, 16);
						MMP += Constants.Rand.Next(50, 52);
						break;
					case 32:
						MHP += Constants.Rand.Next(20, 24);
						MMP += Constants.Rand.Next(42, 44);
						break;
				}
			}

			MMP += _parent.BasicStats.nINT / 10;
		}

		private int GetSpGainOnLevel()
		{
			if (Job <= 2214 && Job >= 2200) return 4;
			if (Job > 3000 && Job < 4000) return 4;

			return 3;
		}

		private int GetApGainOnLevel()
		{
			var kocBonus = JobLogic.IsKOC(Job) && Level < 70 ? 1 : 0;

			// double ap gain every 10 levels above level 10
			if (Level > 10 && Level % 10 == 0)
			{
				return 10 + kocBonus;
			}

			return 5 + kocBonus;
		}

		public void AutoJobAdvanceEvent(Character c)
		{
			if (JobLogic.IsMaxJob(Job) || Level < 20) return;

			// handle dual blade
			if (SubJob == 1)
			{
				switch (Job)
				{
					case 0 when Level >= 10:
						Job = 400;
						break;
					case 400 when Level >= 20:
						Job = 430;
						break;
					case 430 when Level >= 30:
					case 431 when Level >= 55:
					case 432 when Level >= 70:
					case 433 when Level >= 120:
						Job += 1;
						break;
				}
			}
			else if (JobLogic.IsEvan(Job))
			{
				switch (Job) // 10, 20, 30, 40, 60, 80, 120, 160
				{
					case 2001 when Level >= 10:
						Job = 2200;
						break;
					case 2200 when Level >= 20:
						Job = 2210;
						break;
					case 2210 when Level >= 30:
					case 2211 when Level >= 40:
					case 2212 when Level >= 50:
					case 2213 when Level >= 60:
					case 2214 when Level >= 80:
					case 2215 when Level >= 100:
					case 2216 when Level >= 120:
					case 2217 when Level >= 160:
						Job += 1;
						break;
				}
			}
			else if (Level < 30) // all other jobs need to be >= level 30 for job adv
			{
				return;
			}
			else if (Job % 100 == 0)
			{
				if (Job > 1000) // these jobs only have one advancement option
				{
					Job += 10;
				}
				else // these jobs have several advancement options
				{
					c.SendMessage("Use command @job whenever you wish to specialize your class.");
				}
			}
			else
			{
				switch (Job % 10)
				{
					case 0 when Level >= 70:
					case 1 when Level >= 120 && !JobLogic.IsKOC(Job):
						Job += 1;
						break;
					case 2:
						// fourth job, do nothing
						break;
					default:
						c.SendMessage("An error has occurred in the auto job advance function.");
						break;
				}
			}
		}

		/// <summary>
		/// Used for parsing client AP distribution requests.
		/// Do not use this for internal changes.
		/// </summary>
		/// <param name="nStatFlag"></param>
		/// <param name="nAmount"></param>
		public void AddStat(ModifyStatFlags nStatFlag, int nAmount)
		{
			switch (nStatFlag)
			{
				case ModifyStatFlags.STR:
					STR += (short)nAmount;
					break;
				case ModifyStatFlags.DEX:
					DEX += (short)nAmount;
					break;
				case ModifyStatFlags.INT:
					INT += (short)nAmount;
					break;
				case ModifyStatFlags.LUK:
					LUK += (short)nAmount;
					break;
				case ModifyStatFlags.MaxHP:
					MHP += nAmount;
					break;
				case ModifyStatFlags.MaxMP: // CQWUser::IncMaxMPVal
					{
						int inc;
						switch (Job / 100 % 10)
						{
							case 0:
								inc = 6;
								break;
							case 1:
								inc = 2;
								break;
							case 2:
								inc = 18;
								break;
							default:
								inc = 10;
								break;
						}
						MMP += inc;
					}
					break;
			}
		}

		public void Encode(COutPacket packet)
		{
			packet.Encode4((int)Flag);

			if ((Flag & ModifyStatFlags.Skin) != 0) packet.Encode1(Skin);
			if ((Flag & ModifyStatFlags.Face) != 0) packet.Encode4(Face);
			if ((Flag & ModifyStatFlags.Hair) != 0) packet.Encode4(Hair);

			if ((Flag & ModifyStatFlags.Pet) != 0) packet.Encode8(_stat.aliPetLockerSN[0]);
			if ((Flag & ModifyStatFlags.Pet2) != 0) packet.Encode8(_stat.aliPetLockerSN[1]);
			if ((Flag & ModifyStatFlags.Pet3) != 0) packet.Encode8(_stat.aliPetLockerSN[2]);

			if ((Flag & ModifyStatFlags.Level) != 0) packet.Encode1(Level);
			if ((Flag & ModifyStatFlags.Job) != 0) packet.Encode2(Job);
			if ((Flag & ModifyStatFlags.STR) != 0) packet.Encode2(STR);
			if ((Flag & ModifyStatFlags.DEX) != 0) packet.Encode2(DEX);
			if ((Flag & ModifyStatFlags.INT) != 0) packet.Encode2(INT);
			if ((Flag & ModifyStatFlags.LUK) != 0) packet.Encode2(LUK);

			if ((Flag & ModifyStatFlags.HP) != 0) packet.Encode4(HP);
			if ((Flag & ModifyStatFlags.MaxHP) != 0) packet.Encode4(MHP); // VERIFY: idk if we send actual mhp or just base mhp here
			if ((Flag & ModifyStatFlags.MP) != 0) packet.Encode4(MP);
			if ((Flag & ModifyStatFlags.MaxMP) != 0) packet.Encode4(MMP);

			if ((Flag & ModifyStatFlags.AP) != 0) packet.Encode2(AP);
			if ((Flag & ModifyStatFlags.SP) != 0)
			{
				if (!JobLogic.IsExtendedSPJob(Job))
				{
					packet.Encode2(SP);
				}
				else
				{
					var size = (byte)JobLogic.GetExtendedSPIndexByJob(Job);

					packet.Encode1(size);

					for (int i = 1; i <= size; i++)
					{
						packet.Encode1((byte)i); // encode index
						packet.Encode1((byte)_stat.extendSP[i]); // encode value
					}
				}
			}

			if ((Flag & ModifyStatFlags.EXP) != 0) packet.Encode4(EXP);
			if ((Flag & ModifyStatFlags.POP) != 0) packet.Encode2(POP);

			if ((Flag & ModifyStatFlags.Money) != 0) packet.Encode4(Money);
			if ((Flag & ModifyStatFlags.TempEXP) != 0) throw new ArgumentOutOfRangeException("Unsupported flag"); // packet.Encode4(TempExp);
		}
	}
}