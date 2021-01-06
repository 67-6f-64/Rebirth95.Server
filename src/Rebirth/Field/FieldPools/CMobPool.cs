using log4net;
using Rebirth.Characters;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using Rebirth.Common.Tools;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.Item.Consume;
using Rebirth.Tools;

namespace Rebirth.Field.FieldPools
{
	public class CMobPool : CObjectPool<CMob>
	{
		public static ILog Log = LogManager.GetLogger(typeof(CMobPool));

		//	public List<CMob> Spawns { get; private set; }

		public List<MOBGEN> aMobGen { get; private set; }

		public int m_nSubMobCount { get; set; }

		private DateTime m_tLastCreateMobTime;

		public bool bHasIntervalMobs { get; private set; }

		private int nInitMobGenCount { get; set; }
		private int nMobGenCount { get; set; }
		private bool bMobGenEnable { get; set; }
		private List<int> aMobGenExcept { get; set; }

		public CMobPool(CField parentField) : base(parentField)
		{
			//Spawns = new List<CMob>();
			aMobGen = new List<MOBGEN>();
			aMobGenExcept = new List<int>();
			m_nSubMobCount = -1;
		}

		public void Update()
		{
			var mobsToRemove = new List<CMob>();

			foreach (var mob in this)
			{
				if (mob.IsTimeToRemove(mob.Template.TemplateId == 9999999))
				{
					mobsToRemove.Add(mob);
				}
				else
				{
					mob.Stats.Update(mob);

					if (mob.Dead)
					{
						mobsToRemove.Add(mob);
					}
				}
			}

			foreach (var mob in mobsToRemove)
			{
				Remove(mob);
			}

			Field.AssignControllerMobs();

			TryCreateMob(false);
		}

		public override void Load(int mapId)
		{
			if (Field.bPauseSpawn) return;

			var aMobInfo = new Dictionary<int, int>(); // mobId, mobCount

			var life = Field.Template.Life.Where(l => l.Type.EqualsIgnoreCase("m")).ToList();

			if (life.Count <= 0) return; // not a natural mob map

			var bHasFlyMobs = false;
			var bHasNonBossMob = false;

			foreach (var mob in life)
			{
				var template = MasterManager.MobTemplates[mob.TemplateId];

				if (template is null)
				{
					Log.Error($"null mob template {mob.TemplateId} in field {Field.MapId}");
					continue;
				}

				var pmg = new MOBGEN
				{
					dwTemplateID = mob.TemplateId,
					X = mob.X,
					F = mob.F,
					tRegenInterval = mob.MobTime * 1000,
				};

				if (mob.MobTime > 180) // seconds
				{
					bHasIntervalMobs = true;
				}

				if (template.MoveAbility == MobMoveType.Fly)
				{
					pmg.Y = mob.Y;
					pmg.FH = 0;
				}
				else
				{
					pmg.FH = mob.Foothold;
					pmg.Y = mob.CY;
				}

				if (pmg.tRegenInterval > 0)
				{
					var tBaseInterval = pmg.tRegenInterval / 10;
					var tRandAddedInterval = 6 * tBaseInterval + 1; // add one to eliminate DBZ exception

					pmg.tRegenAfter = DateTime.Now.AddMilliseconds(tBaseInterval + Constants.Rand.Next() % tRandAddedInterval);
				}

				aMobGen.Add(pmg);
				nInitMobGenCount += 1;

				if (template.MoveAbility == MobMoveType.Fly)
				{
					bHasFlyMobs = true;
				}

				if (!template.Boss)
				{
					bHasNonBossMob = true;
				}
			}

			// if (false)
			if (bHasNonBossMob && !bHasFlyMobs && Constants.CustomSpawn_Enabled && Field.Template.FieldType == 0 && !Field.IsInstanced)
			{
				foreach (var mob in aMobGen)
				{
					var pTemplate = MasterManager.MobTemplates[mob.dwTemplateID];

					// we don't want to spawn bosses or standing mobs
					if (pTemplate.Boss || pTemplate.MoveAbility == MobMoveType.Stop) continue; 

					if (aMobInfo.ContainsKey(mob.dwTemplateID))
					{
						aMobInfo[mob.dwTemplateID] += 1;
					}
					else
					{
						aMobInfo.Add(mob.dwTemplateID, 1);
					}
				}

				GenerateSpawnPointsFromFH(aMobInfo);
			}

			nMobGenCount = nInitMobGenCount;
			TryCreateMob(true);
		}

		private void GenerateSpawnPointsFromFH(Dictionary<int, int> aMobInfo)
		{
			if (aMobInfo.Count <= 0) return; // TODO consider changing to 1-2

			var nBaseMobCount = aMobGen.Count;

			var footholds = Field.Footholds.NonWallFHs.Where(fh => fh.Slope <= Constants.CustomSpawn_MaxSlope);

			var nMapFHCombinedLength = footholds.Sum(fh => fh.Length);

			var nTotalMobCount = nMapFHCombinedLength / 125 - nBaseMobCount * 0.65;

			var footholdCandidates = Field.Footholds.NonWallFHs.Where(fh => fh.Slope <= Constants.CustomSpawn_MaxSlope);

			var min = footholdCandidates.Max(f => f.Length) * 0.25;

			while (aMobGen.Count < nTotalMobCount)
			{
				foreach (var fh in footholdCandidates)
				{
					if (fh.Length <= min) continue;

					var randMob = aMobInfo.Random();

					var weightedAvg = (double)randMob.Value / nBaseMobCount;

					// count divided by total
					var spawnOdds = weightedAvg * Constants.CustomerSpawn_OddsLimit * fh.Length * 0.01; // weighted average with a cap

					if (Constants.Rand.NextDouble() > spawnOdds) continue;

					var pmg = new MOBGEN
					{
						dwTemplateID = randMob.Key,
						X = (short)((fh.X1 + fh.X2) / 2),
						FH = fh.Id,
						Y = (short)(((fh.Y1 + fh.Y2) / 2) - 15), // offset so mobs dont fall thru map (we dont check for flying mobs cuz we dont customize them)
						F = false, // TODO figure out how to determine the direction the mob faces.. it may not matter for regular mobs
						tRegenInterval = 20_000 // 20 sec
					};

					if (pmg.tRegenInterval > 0)
					{
						var tBaseInterval = pmg.tRegenInterval / 10;
						var tRandAddedInterval = 6 * tBaseInterval + 1; // add one to eliminate DBZ exception

						pmg.tRegenAfter = DateTime.Now.AddMilliseconds(tBaseInterval + Constants.Rand.Next() % tRandAddedInterval);
					}

					aMobGen.Add(pmg);
					nInitMobGenCount += 1;

					// make the exact mob count vary by a lil
					if (aMobGen.Count > nTotalMobCount && Constants.Rand.Next(100) < 25)
						return;
				}
			}
		}

		//--------------------------------------------------

		public void Move(Character pUser, CInPacket iPacket)
		{
			// TODO: Set person whose client sent this as controller ( they may not be )
			//Recv [CP_MobMove] [E3 00] [11 27 00 00] [12 00] 00 1D 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 CC DD FF 00 CC DD FF 00 BA 16 D8 40 09 05 5E 00 00 00 00 00 01 00 09 05 5E 00 00 00 00 00 DC 00 00 00 00 00 05 38 04 00 09 05 5E 00 09 05 5E 00 01 01 01 00 9A 48 03 00

			var pMonster = this[iPacket.Decode4()];

			// TODO don't block the packet if the sender isnt the controller
			if (pMonster == null || pMonster.Controller.dwId != pUser.dwId) return;

			var nMobCtrlSN = iPacket.Decode2();
			var dwFlag = iPacket.Decode1(); // bSomeRand | 4 * (bRushMove | 2 * (bRiseByToss | 2 * nMobCtrlState)); 

			var bNextAttackPossible = (dwFlag & 0xF) != 0; // BMS

			var nActionAndDir = iPacket.Decode1();

			var dwData = iPacket.Decode4(); // !CMob::DoSkill(v7, (unsigned __int8)dwData, BYTE1(dwData), dwData >> 16)

			var nMultiTargetSize = iPacket.Decode4();

			for (int i = 0; i < nMultiTargetSize; i++)
			{
				iPacket.Decode4(); //aMultiTargetForBall[i].x
				iPacket.Decode4(); //aMultiTargetForBall[i].y
			}

			var nRandTimeSize = iPacket.Decode4();

			for (int i = 0; i < nRandTimeSize; i++)
			{
				iPacket.Decode4(); //m_aRandTimeforAreaAttack[i]
			}

			var nMoveFlags = iPacket.Decode1();

			iPacket.Decode4(); //CVecCtrlMob::GetHackedCode
			iPacket.Decode4(); //CVecCtrlMob::MoveContext::FlyContext->ptTargey.x
			iPacket.Decode4(); //CVecCtrlMob::MoveContext::FlyContext->ptTargey.y
			iPacket.Decode4(); //dwHackedCodeCRC

			// TODO change controller logic
			//if (mob.getController().getUser() != ctrl && ((mobCtrlState & 0xF0) == 0 || mob.isNextAttackPossible()
			//	|| !lifePool.changeMobController(ctrl.getCharacterID(), mob, true)))
			//{
			//	mob.sendChangeControllerPacket(ctrl, (byte)0);
			//	return;
			//}

			var nAction = (byte)(nActionAndDir == 0xFF // -1
				? 0xFF : nActionAndDir >> 1);

			var skillCommand = new SkillCommand(); // populated by OnMobMove()

			if (pMonster.OnMobMove(bNextAttackPossible, nAction, dwData, skillCommand))
			{
				pUser.SendPacket(CPacket.CMobPool.MobMoveAck(pMonster.dwId, nMobCtrlSN, bNextAttackPossible, pMonster.Stats.MP, skillCommand));

				var oPacket = CPacket.CMobPool.MobMove(pMonster.dwId, bNextAttackPossible, nActionAndDir, dwData);

				var len = pMonster.Position.UpdateMovePath(oPacket, iPacket); // decode & encode movepath

				iPacket.Decode1(); // bChasing
				iPacket.Decode1(); // pTarget != 0
				iPacket.Decode1(); // bChasing
				iPacket.Decode1(); // bChasingHack
				iPacket.Decode4(); // tChaseDuration

				if (len > 0)
				{
					Field.Broadcast(oPacket, pUser);
				}
			}
		}

		//--------------------------------------------------

		/// <summary>
		/// This iterates the mobgen and spawns the mobs that can be spawned.
		/// </summary>
		/// <remarks>
		/// Previously referred to as RedistributeLife()
		/// </remarks>
		/// <param name="bReset">This will force the map to reset its spawns regardless of current spawn cooldowns.</param>
		public void TryCreateMob(bool bReset)
		{
			if (Field.bPauseSpawn) return;

			if (bReset)
			{
				SetMobGen(true, 0);
			}

			if (aMobGen.Count <= 0 || nMobGenCount == 0) return;

			var tTime = DateTime.Now;

			if (!bReset && m_tLastCreateMobTime.MillisSinceStart() < 7000) return;

			m_tLastCreateMobTime = DateTime.Now;

			var nMobCapacity = aMobGen.Count * 0.75;

			var nMobCount = nMobCapacity - Count;

			if (nMobCount <= 0) return;

			var aMobGen_ = new List<MOBGEN>();
			var aPts = new List<TagPoint>();
			var nCount = 0;

			if (bMobGenEnable)
			{
				aPts.AddRange(this.Select(pMob => pMob.Position.CurrentXY));

				var bCheckArea = false;
				foreach (var pMG in aMobGen)
				{
					if (!aMobGenExcept.Contains(pMG.dwTemplateID))
					{
						if (pMG.tRegenInterval == 0)
						{
							bCheckArea = true;
						}
						else if (pMG.tRegenInterval > 0)
						{
							if (bReset)
							{
								var tInterval = pMG.tRegenInterval >> 6;

								// from 22-130 minutes on BF to 100-603 seconds

								tInterval += Constants.Rand.Next() % (6 * (tInterval >> 2));

								pMG.tRegenAfter = DateTime.Now.AddMilliseconds(tInterval);
							}

							if (pMG.nMobCount == 0 && (tTime - pMG.tRegenAfter).TotalMilliseconds >= 0)
							{
								aMobGen_.Add(pMG);
								aPts.Add(new TagPoint((short)pMG.X, (short)pMG.Y));
							}
						}
					}

					if (bCheckArea)
					{
						var rcArea = new TagRect(pMG.X - 100, pMG.Y - 100, pMG.X + 100, pMG.Y + 100);
						var bAdd = true;
						var nIdx = 0;

						while (nIdx < aPts.Count)
						{
							var _pt = aPts[nIdx];

							if (rcArea.PointInRect(_pt))
							{
								bAdd = false;
								break;
							}

							nIdx += 1;
						}

						if (bAdd)
						{
							aMobGen_.Add(pMG);
							aPts.Add(new TagPoint((short)pMG.X, (short)pMG.Y));
						}

						bCheckArea = false;
					}

					nCount += 1;

					if (nCount >= nMobGenCount) break;
				}
			}

			if (aMobGen_.Count > 0)
			{
				while (true)
				{
					if (aMobGen_.Count <= 0) break;

					var nIdx = Math.Abs(Constants.Rand.Next() % aMobGen_.Count);
					var pMG = aMobGen_.ElementAtOrDefault(nIdx);

					aMobGen_.RemoveAt(nIdx);

					if (CreateMob(pMG.dwTemplateID, pMG, pMG.X, pMG.Y, pMG.FH, (byte)MobAppearType.MOBAPPEAR_REGEN, 0, pMG.F ? 1 : 0, 0, null))
					{
						nMobCount -= 1;
					}

					if (nMobCount <= 0) break;
				}
			}

			aMobGen_.Clear();
		}

		public bool CreateMob(int dwTemplateID, MOBGEN pmg, int ptX, int ptY, int nFH, byte nAppearType, int dwOption, int nDir, MobType nMobType, Character c)
		{
			// TODO check control count and set null if control count >= 50

			if (pmg != null && pmg.tRegenInterval < 0)
			{
				c = null;
			}
			else if (c is null)
			{
				c = Field.Users.Random(); // todo
			}

			var template = MasterManager.MobTemplates[dwTemplateID];

			if (template != null) //&& c != null) // bms uses controller objects and checks for them to be non-null but we dont do that so..
			{
				var mob = new CMob(dwTemplateID);
				mob.Init(pmg, nFH);

				MoveActionType nMoveAction;

				if (mob.Template.MoveAbility == MobMoveType.Fly)
				{
					nMoveAction = MoveActionType.MA_FLY1;
				}
				else
				{
					nMoveAction = mob.Template.MoveAbility == MobMoveType.Stop
						? MoveActionType.MA_STAND
						: MoveActionType.MA_MOVE;
				}

				mob.SetMovePos(ptX, ptY, nDir & 1 | 2 * (int)nMoveAction, nFH);

				if (nMobType == MobType.SubMob)
				{
					if (m_nSubMobCount < 0) m_nSubMobCount = 1;
					else m_nSubMobCount += 1;
				}

				if (nAppearType == 0) nAppearType = 0xFE; // regular animation

				mob.m_nMobType = nMobType;
				mob.m_nSummonType = nAppearType; // theres an enum for this but the values can be outside of the enum so we skip using it
				mob.m_dwSummonOption = dwOption;

				Add(mob); // add to pool, assign field and id

				if (mob.m_nSummonType != 0xFC) // suspended
				{
					mob.m_nSummonType = 0xFF; // normal
					mob.m_dwSummonOption = 0;
				}

				// controller has to be set after mob has been inserted in pool
				mob.SetController(c, MobCtrlType.Active_Int);

				mob.BroadcastHP(); // sends boss hp bar

				if (Field.nVelocityControllerdwId > 0 && !mob.Template.Boss)
				{
					var user = Field.Users[Field.nVelocityControllerdwId];

					if (user != null)
					{
						var skill = user.Skills.Get((int)Skills.MECHANIC_VELOCITY_CONTROLER);

						mob.TryApplySkillDamageStatus(user, skill.nSkillID, skill.nSLV, 0);
					}
				}

				return true;
			}

			return false;
		}

		public void SetMobGen(bool bMobGen, int nMobTemplateID)
		{
			if (nMobTemplateID != 0)
			{
				if (bMobGen)
				{
					aMobGenExcept.Remove(nMobTemplateID);
				}
				else
				{
					if (!aMobGenExcept.Contains(nMobTemplateID))
					{
						aMobGenExcept.Add(nMobTemplateID);
					}
				}
			}
			else
			{
				bMobGenEnable = bMobGen;
			}
		}

		public void RedistributeLife()
		{
			// TODO this redistributes controllers in BMS
		}

		// CLifePool::OnMobSelfDestruct
		public void OnMobSelfDestruct(int dwMobID)
		{
			var mob = this[dwMobID];

			// prolly client lagging or something
			if (mob is null) return;

			if ((mob.Template.SelfDestructActionType & 2) != 0)
			{
				mob.DoSelfDestruct();
				Remove(mob);
			}
			else
			{
				// PE
			}
		}

		public bool OnMobSummonItemUseRequest(int nItemID, short nX, short nY)
		{
			var template = MasterManager.ItemTemplate(nItemID) as ConsumeItemTemplate;

			if (template is null) return false;

			var fh = Field.Footholds.FindBelow(nX, nY);

			var mobs = new List<int>(template.SummoningSackIDs.Length);

			for (var i = 0; i < template.SummoningSackIDs.Length; i++)
			{
				if (template.SummoningSackProbs.Length < i) break;

				if (template.SummoningSackProbs[i] != 100
					&& Constants.Rand.Next(100) < template.SummoningSackProbs[i]) break;

				mobs.Add(template.SummoningSackIDs[i]);
			}

			var bSuccess = true;

			foreach (var mob in mobs)
			{
				if (!CreateMob(mob, null, nX, fh.Y1, fh.Id, (byte)template.InfoType, 0, 0, MobType.Normal, null))
				{
					bSuccess = false;
					break;
				}
			}

			return bSuccess;
		}

		public void RemoveAll(bool bDropDrops = true)
		{
			foreach (var mob in this.ToList())
			{
				if (!bDropDrops) mob.bRewardsDistributed = true;

				Remove(mob);
			}
		}

		public override void Dispose()
		{
			Clear();
			aMobGen.Clear();
			aMobGenExcept.Clear();
		}

		protected override void ClearItems()
		{
			this.ForEach(mob =>
			{
				mob.LeaveFieldType = MobLeaveFieldType.MOBLEAVEFIELD_SUMMONTIMEOUT;
				Field.Broadcast(mob.MakeLeaveFieldPacket());
				mob.SetController(null, 0);
				mob.Attackers.Clear();
			});

			base.ClearItems();
		}

		protected override void InsertItem(int index, CMob mob)
		{
			// must perform base call first because the following call might 
			//		modify the list size which would screw up our index
			base.InsertItem(index, mob);

			Field.OnInsertMob(mob);

			Field.Broadcast(mob.MakeEnterFieldPacket());
		}

		// CLifePool::RemoveMob(CLifePool *this, CMob *pMob)
		protected override void RemoveItem(int index)
		{
			var removedMob = GetAtIndex(index);

			try
			{
				// field and id are reset in the base.RemoveItem()
				//   so we need to broadcast the leavefield packet and 
				//   distribute rewards before we remove from the pool
				if (removedMob != null)
				{
					Field.OnMobDie(removedMob);

					removedMob.SetRemoved();

					foreach (var item in removedMob.Template.Revive)
					{
						CreateMob(item, removedMob.MobGen,
							removedMob.Position.X, removedMob.Position.Y, removedMob.Position.Foothold,
							0xFD, removedMob.dwId, removedMob.Position.MoveAction & 1, MobType.Normal, removedMob.Controller);
					}

					removedMob.SendMobHPEnd();

					Field.Broadcast(removedMob.MakeLeaveFieldPacket());

					if (removedMob.m_nMobType == MobType.SubMob)
					{
						m_nSubMobCount -= 1;
					}

					if (m_nSubMobCount == 0)
					{
						foreach (var loopMob in this)
						{
							if (loopMob.m_nMobType == MobType.ParentMob)
							{
								loopMob.SuspendReset(true);
								break;
							}
						}
					}

					removedMob.SetController(null, 0); // last operation
				}
			}
			catch (Exception ex)
			{
				if (removedMob != null) // make super sure this fucker is gone
				{
					Field.Broadcast(removedMob.MakeLeaveFieldPacket());
					removedMob.SetRemoved();
				}

				Log.Error(ex);
			}

			base.RemoveItem(index);
		}

		public void Reset()
		{
			Clear();
			TryCreateMob(true);
		}

		protected override int GetKeyForItem(CMob item) => item.dwId;
	}
}
