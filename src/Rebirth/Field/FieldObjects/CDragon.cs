using Rebirth.Characters;
using Rebirth.Network;
using Rebirth.Server.Center;

namespace Rebirth.Field.FieldObjects
{
	public class CDragon : CFieldObj
	{
		public Character Parent { get; private set; }
		public int dwParentId => Parent.dwId;

		public CDragon(Character parent)
		{
			Parent = parent;
		}

		public override void Dispose()
		{
			Parent = null;
			Position = null;

			base.Dispose();
		}

		public void Move(CInPacket p)
		{
			Parent.Field.Broadcast(DragonMove(p), Parent);
		}

		private COutPacket DragonMove(CInPacket iPacket)
		{
			var oPacket = new COutPacket(SendOps.LP_DragonMove);
			oPacket.Encode4(dwParentId);
			Position.UpdateMovePath(oPacket, iPacket);
			return oPacket;
		}

		/// <summary>
		/// Sends dragon spawn packet to everyone in parent field
		/// </summary>
		public void SpawnDragonToMap()
		{
			Field = Parent.Field;
			Position.ResetPosTo(Parent.Position);
			Parent.Field.Broadcast(MakeEnterFieldPacket());
		}

		public override COutPacket MakeEnterFieldPacket()
		{
			var p = new COutPacket(SendOps.LP_DragonEnterField);
			p.Encode4(dwParentId);
			p.Encode4(Position.X);
			p.Encode4(Position.Y);
			p.Encode1(Position.MoveAction);
			p.Encode2(0); //Not used from what it seems
			p.Encode2(Parent.Stats.nJob);
			return p;
		}

		public override COutPacket MakeLeaveFieldPacket()
		{
			//This packet is not handled inside of maplestory even tho opcode exists
			var p = new COutPacket(SendOps.LP_DragonLeaveField);
			p.Encode4(dwParentId);
			return p;
		}
	}
}
