using Autofac;
using Rebirth.Characters;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Scripts;
using System;

namespace Rebirth.Field.FieldPools
{
	public class CReactorPool : CObjectPool<CReactor>
	{
		public CReactorPool(CField parentField)
			: base(parentField) { }

		public void OnPacket(RecvOps opCode, CInPacket p, Character c)
		{
			if (c.Stats.nHP <= 0) return;

			var dwID = p.Decode4();
			var item = this[dwID];

			if (item is null) return;

			switch (opCode)
			{
				case RecvOps.CP_ReactorHit:
					item.Hit(c, p);
					break;
				case RecvOps.CP_ReactorTouch:
					item.Touch(c, p);
					break;
			}
			var provider = ServerApp.Container.Resolve<ScriptManager>();
			var script = provider.GetReactorScript
				(item.sName, item, c.Socket, opCode == RecvOps.CP_ReactorHit);

			script.Execute();
		}

		public override void Load(int mapId)
		{
			foreach (var item in Field.Template.Reactors)
			{
				var entry = new CReactor(item)
				{
					Position = new CMovePath
					{
						X = item.X,
						Y = item.Y,
					},
					bFlipped = item.F,
					bActive = true,
				};

				Add(entry);
			}
		}

		public void Reset()
		{
			Clear();
			Load(Field.MapId);
		}

		public void RedistributeLife()
		{
			foreach (var reactor in this)
			{
				if (reactor.nSpawnIndex < 0) continue;

				if (!reactor.bActive)
				{
					if (reactor.tReactorTime > 0 && reactor.tLastDead.AddedSecondsExpired(reactor.tReactorTime))
					{
						reactor.OnHitReactor(null, 0, 0);
					}
				}
			}
		}

		protected override void InsertItem(int index, CReactor item)
		{
			base.InsertItem(index, item);

			item.bActive = true;
			Field.Broadcast(item.MakeEnterFieldPacket());
		}

		protected override void RemoveItem(int index)
		{
			var item = GetAtIndex(index);

			if (item != null)
			{
				Field.Broadcast(item.MakeLeaveFieldPacket());
			}

			base.RemoveItem(index);
		}
	}
}
