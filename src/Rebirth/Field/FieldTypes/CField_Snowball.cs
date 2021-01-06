using System;
using System.Collections.Generic;
using Rebirth.Characters;
using Rebirth.Field.FieldObjects;
using Rebirth.Field.FieldTypes.Custom;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Game;

namespace Rebirth.Field.FieldTypes
{
	public sealed class CField_Snowball : CField_CommunityEvent
	{
		protected override int nEntranceMap => 109060000;
		public override string sEventName => "SnowBall";

		// const data is from map wz
		public const int SNOWMAN_BASE_HP = 7500; // TODO
		public const int DAMAGE_SNOWBALL = 10; // TODO
		public const int SNOWMAN_WAIT_DURATION_MILLIS = 10000; // millis

		private readonly int[] nSectionX = new int[3]
		{
			45, 290, 560
		};

		public readonly int[] nDamageSnowMan = new int[2]
		{
			15,
			45
		};

		private readonly int nSpeed = 150;

		// end const data

		public bool bConcluded { get; set; }
		public bool[] bSnowBallStopped { get; private set; }
		public CSnowBall[] aSnowBall { get; private set; } // top team is team 1, bottom team is team 
		public int[] nSnowManCurHP { get; set; }
		public DateTime[] tSnowBallWaitTime { get; private set; }
		/// <summary>
		/// Keeps track of the most recent section that each snowball has entered.
		/// </summary>
		public int[] nSectionOld { get; set; }
		public int nState { get; set; }
		public List<int>[] aaCharacterID { get; set; }

		// CField_SnowBall::CField_SnowBall
		// CField_SnowBall::OnStart(CField_SnowBall *this, CUser *pUser, CInPacket *iPacket)
		public CField_Snowball(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId) { }

		public override void Setup()
		{
			aSnowBall = new CSnowBall[2];
			bSnowBallStopped = new bool[2];
			nSectionOld = new int[2];
			nSnowManCurHP = new int[2];
			tSnowBallWaitTime = new DateTime[2];
			aaCharacterID = new List<int>[2]
			{
				new List<int>(),
				new List<int>()
			};

			nState = 0;
			bConcluded = false;
			base.Setup();
		}

		public void Start()
		{
			CloseRegistration();

			nState = 1;

			for (var i = 0; i < 2; i++)
			{
				aSnowBall[i] = new CSnowBall(aaCharacterID[0].Count + aaCharacterID[1].Count);
				bSnowBallStopped[i] = false;
				nSectionOld[i] = 0;
				nSnowManCurHP[i] = SNOWMAN_BASE_HP;
				tSnowBallWaitTime[i] = DateTime.Now;
			}

			CreateFieldClock(600); // 10 min

			BroadcastState(true);
		}

		public void BroadcastState(bool bFirst = false)
		{
			var statePacket = new COutPacket(SendOps.LP_SnowBallState);
			EncodeState(statePacket);

			if (bFirst)
			{
				statePacket.Encode2(DAMAGE_SNOWBALL);
				statePacket.Encode2((short)nDamageSnowMan[0]);
				statePacket.Encode2((short)nDamageSnowMan[1]);
			}

			Broadcast(statePacket);

			if (bConcluded)
			{
				var nWinIdx = nState == 3 ? 1 : 0;
				var nLoseIdx = nWinIdx == 3 ? 0 : 1;

				var effect = new FieldEffectPacket(FieldEffectTypes.FieldEffect.Screen)
				{ sName = sEffectWin };

				var sound = new FieldEffectPacket(FieldEffectTypes.FieldEffect.Sound)
				{ sName = sSoundWin };

				var winners = new List<Character>();

				foreach (var userId in aaCharacterID[nWinIdx])
				{
					var user = Users.GetOrDefault(userId);

					if (user is null) continue;

					winners.Add(user);

					effect.Broadcast(user, true);
					sound.Broadcast(user, true);
				}

				DistributeRewards(winners);

				effect.sName = sEffectLose;
				sound.sName = sSoundLose;

				foreach (var userId in aaCharacterID[nLoseIdx])
				{
					var user = Users.GetOrDefault(userId);

					if (user is null) continue;

					effect.Broadcast(user, true);
					sound.Broadcast(user, true);
				}

				CreateFieldClock(5); // warp out
			}
		}

		protected override void OnClockEnd()
		{
			// no need to call base class because both functions (EndEvent and Start) modify the clock
			if (nState >= 1)
			{
				EndEvent();
			}
			else
			{
				Start();
			}
		}

		public override void OnUserLeave(Character pUser, bool bOnMigrateOut = false)
		{
			if (!aaCharacterID[0].Remove(pUser.dwId))
			{
				aaCharacterID[1].Remove(pUser.dwId);
			}

			base.OnUserLeave(pUser);
		}

		protected override void OnUserEnter(Character pUser)
		{
			base.OnUserEnter(pUser);

			if (nState == 1)
			{
				var entryPacket = new COutPacket(SendOps.LP_SnowBallState);
				EncodeState(entryPacket);
				entryPacket.Encode2(DAMAGE_SNOWBALL);
				entryPacket.Encode2((short)nDamageSnowMan[0]);
				entryPacket.Encode2((short)nDamageSnowMan[1]);
				pUser.SendPacket(entryPacket);
			}
		}

		public override CPortal GetStartPoint(Character c)
		{
			var bottomCharCount = aaCharacterID[0].Count;
			var topCharCount = aaCharacterID[1].Count;

			string sPortalName;

			if (bottomCharCount > topCharCount)
			{
				sPortalName = "st01";
				aaCharacterID[1].Add(c.dwId);
			}
			else
			{
				sPortalName = "st00";
				aaCharacterID[0].Add(c.dwId);
			}

			return Portals.FindPortal(sPortalName);
		}

		private void EncodeState(COutPacket p)
		{
			p.Encode1((byte)nState);
			p.Encode4(100 * nSnowManCurHP[0] / SNOWMAN_BASE_HP);
			p.Encode4(100 * nSnowManCurHP[1] / SNOWMAN_BASE_HP);
			aSnowBall[0].Encode(p);
			aSnowBall[1].Encode(p);
		}

		// CField_SnowBall::Update(CField_SnowBall *this, int tCur)
		public override void Update()
		{
			UpdatePosition();
			base.Update();
		}

		// CField_SnowBall::UpdatePosition(CField_SnowBall *this, int tCur)
		private bool UpdatePosition()
		{
			if (nState == 0) return false;

			if (nState == 1)
			{
				var bChanged = aSnowBall[0].Update() || aSnowBall[1].Update();

				TryConclude();

				if (bChanged || nState != 1)
				{
					BroadcastState();
				}

				for (int i = 0; i < 2; i++)
				{
					var section = IsInSection(i);

					if (nSectionOld[i] != section)
					{
						var sectionUpdatePacket = new COutPacket(SendOps.LP_SnowBallMsg);
						sectionUpdatePacket.Encode1((byte)i);
						sectionUpdatePacket.Encode1((byte)section);
						Broadcast(sectionUpdatePacket);

						nSectionOld[i] = section;
					}
				}
			}

			return nState == 1;
		}

		/// <summary>
		/// Returns section that the given snowball is in
		/// </summary>
		/// <param name="nSide">Snowball</param>
		/// <returns>Integer ranging from 0 to 3</returns>
		private int IsInSection(int nSide)
		{
			// CField_SnowBall::IsInSection(CField_SnowBall *this, int nSide)

			var nCurPos = aSnowBall[nSide].nXPos;

			for (int i = 0; i < nSectionX.Length; i++)
			{
				if (nSectionX[i] > nCurPos) return i;
			}

			return 3;
		}

		// CField_SnowBall::SnowManHit(CField_SnowBall *this, int nSide, int nDelta)
		public void SnowManHit(int nSide, int nDamage)
		{
			if (nState == 1 && UpdatePosition())
			{
				var nCurHP = nSnowManCurHP[nSide];

				nSnowManCurHP[nSide] = nCurHP - nDamage;

				if (nCurHP != nCurHP - nDamage)
				{
					ValidateSnomanHP();
				}
			}
		}

		// CField_SnowBall::ValidateSnowManHP(CField_SnowBall *this)
		private void ValidateSnomanHP()
		{
			for (int i = 0; i < 2; i++)
			{
				if (nSnowManCurHP[i] <= 0)
				{
					var nSide = i == 0 ? 1 : 0;

					var snowballPacket = new COutPacket(SendOps.LP_SnowBallMsg);
					snowballPacket.Encode1((byte)nSide);
					snowballPacket.Encode1(4);
					Broadcast(snowballPacket);

					aSnowBall[nSide].nHP = aSnowBall[nSide].nSnowBallMaxHP;
					tSnowBallWaitTime[nSide] = DateTime.Now;
					bSnowBallStopped[nSide] = true;
				}
			}

			BroadcastState();
		}

		// CField_SnowBall::SnowBallHit(CField_SnowBall *this, int nSide, int nDelta)
		public void SnowBallHit(int nSide, int nDamage)
		{
			if (nState == 1 && UpdatePosition())
			{
				var snowball = aSnowBall[nSide];
				var nOldHP = aSnowBall[nSide].nHP / 1000;
				var nNewHP = snowball.nHP + nDamage * nSpeed / 100;

				if (nNewHP <= 0) nNewHP = 0;
				if (nNewHP > snowball.nSnowBallMaxHP) nNewHP = snowball.nSnowBallMaxHP;

				snowball.nHP = nNewHP;

				if (nOldHP != nNewHP / 1000)
				{
					BroadcastState();
				}
			}
		}

		protected override void Reset(bool bFromDispose = false)
		{
			// clear collections
			aSnowBall = null;
			aaCharacterID[0].Clear();
			aaCharacterID[1].Clear();
			aaCharacterID = null;
			bSnowBallStopped = null;
			nSnowManCurHP = null;
			base.Reset(bFromDispose);
		}

		/// <summary>
		/// Changes game state if either team has won.
		/// State is 2 if bottom wins.
		/// State is 3 if top wins.
		/// </summary>
		private void TryConclude()
		{
			if (nState == 1)
			{
				if (aSnowBall[0].nXPos >= aSnowBall[0].nSnowballFinishLine)
				{
					nState = 2;
				}
				else if (aSnowBall[1].nXPos >= aSnowBall[1].nSnowballFinishLine)
				{
					nState = 3;
				}
				else
				{
					return;
				}

				bConcluded = true;
				BroadcastState();
			}
		}

		public override void DistributeRewards(List<Character> charsTo)
		{
			// TODO
		}
	}
}
