using Rebirth.Characters;
using Rebirth.Network;
using System;
using Rebirth.Provider.Template.Map;
using Rebirth.Server.Center;
using System.Linq;
using Rebirth.Provider.Template.Reactor;

namespace Rebirth.Field.FieldObjects
{
	public class CReactor : CFieldObj
	{
		/// <summary>
		/// Template containing data that is unique to this reactor in this map.
		/// </summary>
		public MapReactorTemplate MapReactorTemplate { get; }
		/// <summary>
		/// Template containing data that is shared among all reactors of this ID.
		/// </summary>
		public ReactorTemplate ReactorTemplate { get; }

		public int nReactorTemplateID => MapReactorTemplate.TemplateId;
		public string sName { get; }
		public int tReactorTime { get; set; }
		public int nSpawnIndex { get; }
		public Character LastHitBy { get; set; } // controller? not really tho cuz they arent controlling it

		public byte nState { get; set; }
		public byte nMaxState { get; }

		public bool bDead => nState >= nMaxState;

		public DateTime tLastDead { get; set; }
		public DateTime tHitCooldown { get; private set; }
		public bool bActive { get; set; }
		public bool bFlipped { get; set; }

		public CReactor(MapReactorTemplate pMapReactorTemplate)
		{
			MapReactorTemplate = pMapReactorTemplate;
			ReactorTemplate = MasterManager.ReactorTemplates[nReactorTemplateID];

			if (ReactorTemplate is null) throw new NullReferenceException(nameof(ReactorTemplate));
			if (pMapReactorTemplate is null) throw new ArgumentNullException(nameof(pMapReactorTemplate));

			nSpawnIndex = pMapReactorTemplate.SpawnIndex;
			tReactorTime = pMapReactorTemplate.ReactorTime;

			tLastDead = DateTime.MinValue;
			tHitCooldown = DateTime.MinValue;

			if (ReactorTemplate.Action.Length > 0)
			{
				sName = ReactorTemplate.Action;
			}
			else if (MapReactorTemplate.Name.Length > 0)
			{
				sName = MapReactorTemplate.Name;
			}
			else
			{
				sName = nReactorTemplateID.ToString();
			}

			nMaxState = (byte)(ReactorTemplate.StateCount - 1);
		}

		public override void Dispose()
		{
			LastHitBy = null;
			base.Dispose();
		}

		public void Hit(Character c, CInPacket p)
		{
			// CReactorPool::FindHitReactor
			// CReactorPool::FindSkillReactor

			var bSkillReactor = p.Decode4() != 0;

			if (bSkillReactor) return; // TODO handle this

			var dwHitOption = p.Decode4(); // v4->dwHitOption = v13 & 1 | 2 * CUser::IsOnFoothold(v3);
			var tActionDelay = p.Decode2();
			var nSkillID = p.Decode4(); // for skill reactors?? idk yet

			OnHitReactor(c, tActionDelay, 0); // TODO event idx
		}

		public void Touch(Character c, CInPacket p)
		{
			var bHasReactor = p.Decode1(); //Confirm the name

			// TODO ??
		}

		/// <summary>
		/// Used by scripts to force reactor state.
		/// No sanity checks are performed.
		/// </summary>
		/// <param name="nNewState">New reactor state</param>
		/// <param name="bActive">If the reactor is active and visible to players.</param>
		/// <param name="bNotify">Whether to notify map of state change.</param>
		public void SetReactorState(byte nNewState, bool bActive, bool bNotify)
		{
			nState = nNewState;

			if (!bActive)
			{
				this.bActive = bActive;
				tLastDead = DateTime.Now;
			}

			if (bNotify)
			{
				Field.Broadcast(CPacket.CReactorPool.ReactorChangeState(this, 0, 0));
			}
		}

		public void OnHitReactor(Character pHitter, short tDelay, byte nEventIdx)
		{
			if (!bActive && pHitter != null) return;
			if (tHitCooldown.MillisUntilEnd() > 0) return;

			LastHitBy = pHitter;

			ReactorTemplate.StateInfo pStateInfo;

			var nStateCount = ReactorTemplate.StateInfoList.Count;
			
			if (nState < nStateCount)
			{
				pStateInfo = ReactorTemplate.StateInfoList[nState];
			}
			else
			{
				pStateInfo = ReactorTemplate.StateInfoList[nStateCount - 1];
			}

			if (nEventIdx > pStateInfo.EventInfos.Count)
			{
				LastHitBy?.SendMessage($"Event index exceeds possible events. Please screenshot and report this. EIDX: {nEventIdx} ECount: {pStateInfo.EventInfos.Count}");
				return;
			}

			if (pStateInfo.EventInfos.Count <= 0)
			{
				nState = 0; // reset
				bActive = true;
			}
			else
			{
				nState = (byte)pStateInfo.EventInfos[nEventIdx].NextState;

				if (ReactorTemplate.StateInfoList[nState].EventInfos.Count <= 0)
				{
					bActive = false;
					tLastDead = DateTime.Now;
					LastHitBy = null;
				}
			}

			tHitCooldown = DateTime.Now.AddMilliseconds(pStateInfo.TimeOut);

			if (nState > nMaxState)
				throw new Exception($"Reactor state miss-match (ID: {nReactorTemplateID} => Field: {Field.MapId}). Current state: {nState}. Max state: {nMaxState}");

			Field.Broadcast(CPacket.CReactorPool.ReactorChangeState(this, tDelay, nEventIdx));
		}

		public override COutPacket MakeEnterFieldPacket()
			=> CPacket.CReactorPool.MakeEnterFieldPacket(this);

		public override COutPacket MakeLeaveFieldPacket()
			=> CPacket.CReactorPool.MakeLeaveFieldPacket(this);
	}
}
