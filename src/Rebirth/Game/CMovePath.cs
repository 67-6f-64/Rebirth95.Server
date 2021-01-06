using Rebirth.Common.Tools;
using Rebirth.Common.Types;
using Rebirth.Network;
using static Rebirth.Common.Types.MovePathAttribute;

namespace Rebirth.Game
{
	public class CMoveElement
	{
		public MovePathAttribute nAttr;

		public short X;
		public short Y;

		public short VX;
		public short VY;

		public short xOffset;
		public short yOffset;

		public short FH;
		public short fhFallStart;
		public short fhLast;

		public byte bStat;

		public byte bMoveAction;
		public short tElapse;
	}

	public class CMovePath
	{
		public short X { get; set; }
		public short Y { get; set; }

		// these aren't needed outside of this class
		private short VX { get; set; }
		private short VY { get; set; }

		public short Foothold { get; set; }
		public byte MoveAction { get; set; }

		public TagPoint CurrentXY => new TagPoint(X, Y);

		public void ResetPosTo(CMovePath from, int yOffset = 20)
		{
			X = from.X;
			Y = (short)(from.Y - yOffset);
			Foothold = from.Foothold;
			// movepath??
		}

		public void EncodePos(COutPacket packet)
		{
			packet.Encode2(X);
			packet.Encode2(Y);
		}

		/// <summary>
		/// Encodes these values:
		/// X, Y, MoveAction, Foothold
		/// </summary>
		/// <param name="p"></param>
		public void Encode(COutPacket p)
		{
			EncodePos(p);
			p.Encode1(MoveAction);
			p.Encode2(Foothold);
		}

		// all movement update handlers should be calling this from now on
		public int UpdateMovePath(COutPacket oPacket, CInPacket iPacket)
		{
			X = iPacket.Decode2(); //ZtlSecureTear_m_x
			Y = iPacket.Decode2(); //ZtlSecureTear_m_y
			VX = iPacket.Decode2(); //ZtlSecureTear_m_vx
			VY = iPacket.Decode2(); //ZtlSecureTear_m_vy

			oPacket.Encode2(X);
			oPacket.Encode2(Y);
			oPacket.Encode2(VX);
			oPacket.Encode2(VY);

			var elems = DecodePath(iPacket);
			EncodePath(oPacket, elems);

			return elems.Length;
		}

		private void EncodePath(COutPacket p, CMoveElement[] cMoveElements)
		{
			var x = X;
			var y = Y;

			var ltX = x;
			var rbX = x;
			var ltY = y;
			var rbY = y;

			p.Encode1((byte)cMoveElements.Length);

			foreach (var elem in cMoveElements)
			{
				p.Encode1((byte)elem.nAttr);

				switch (elem.nAttr)
				{
					case MPA_NORMAL:
					case MPA_HANGONBACK:
					case MPA_FALLDOWN:
					case MPA_WINGS:
					case MPA_MOB_ATTACK_RUSH:
					case MPA_MOB_ATTACK_RUSH_STOP:
						p.Encode2(elem.X);
						p.Encode2(elem.Y);
						p.Encode2(elem.VX);
						p.Encode2(elem.VY);
						p.Encode2(elem.FH);

						if (elem.nAttr == MPA_FALLDOWN)
							p.Encode2(elem.fhFallStart);

						p.Encode2(elem.xOffset);
						p.Encode2(elem.yOffset);
						break;
					case MPA_JUMP:
					case MPA_IMPACT:
					case MPA_STARTWINGS:
					case MPA_MOB_TOSS:
					case MPA_DASH_SLIDE:
					case MPA_MOB_LADDER:
					case MPA_MOB_RIGHTANGLE:
					case MPA_MOB_STOPNODE_START:
					case MPA_MOB_BEFORE_NODE:
						p.Encode2(elem.VX);
						p.Encode2(elem.VY);
						break;
					case MPA_IMMEDIATE:
					case MPA_TELEPORT:
					case MPA_ASSAULTER:
					case MPA_ASSASSINATION:
					case MPA_RUSH:
					case MPA_SITDOWN:
						p.Encode2(elem.X);
						p.Encode2(elem.Y);
						p.Encode2(elem.FH);
						break;
					case MPA_STATCHANGE:
						p.Encode1(elem.bStat);
						break;
					case MPA_STARTFALLDOWN:
						p.Encode2(elem.VX);
						p.Encode2(elem.VY);

						p.Encode2(elem.fhFallStart);
						break;
					case MPA_FLYING_BLOCK:
						p.Encode2(elem.X);
						p.Encode2(elem.Y);

						p.Encode2(elem.VX);
						p.Encode2(elem.VY);
						break;
					//case MPA_FLASHJUMP:
					//case MPA_ROCKET_BOOSTER:
					//case MPA_BACKSTEP_SHOT:
					//case MPA_MOBPOWERKNOCKBACK:
					//case MPA_VERTICALJUMP:
					//case MPA_CUSTOMIMPACT:
					//case MPA_COMBATSTEP:
					//case MPA_HIT:
					//case MPA_TIMEBOMBATTACK:
					//case MPA_SNOWBALLTOUCH:
					//case MPA_BUFFZONEEFFECT:
					//	break;
					default:
						// there should be no default, all cases are covered
						break;
				}

				if (elem.nAttr == MPA_STATCHANGE) continue;

				p.Encode1(elem.bMoveAction);
				p.Encode2(elem.tElapse);

				var newX = elem.X;
				var newY = elem.Y;

				if (newX < ltX) ltX = newX;
				if (newX < rbX) rbX = newX;

				if (newY < ltY) ltY = newY;
				if (newY < rbY) rbY = newY;

				//if (CClientOptMan::GetOpt(TSingleton < CClientOptMan >::ms_pInstance, 2u)) // active this if we start sendign Opt in SetField packet
				//{
				//	v9->usRandCnt = CInPacket::Decode2(v3);
				//	v9->usActualRandCnt = CInPacket::Decode2(v3);
				//}
			}
		}

		private CMoveElement[] DecodePath(CInPacket p)
		{
			var oldX = X;
			var oldY = Y;
			var oldVX = VX;
			var oldVY = VY;

			var uCount = p.Decode1(); //m_lElem._m_uCount

			var elements = new CMoveElement[uCount];

			for (var i = 0; i < uCount; i++)
			{
				var e = new CMoveElement();

				e.nAttr = (MovePathAttribute)p.Decode1();
				switch (e.nAttr)
				{
					case MPA_NORMAL:
					case MPA_HANGONBACK:
					case MPA_FALLDOWN:
					case MPA_WINGS:
					case MPA_MOB_ATTACK_RUSH:
					case MPA_MOB_ATTACK_RUSH_STOP:
						e.X = p.Decode2();
						e.Y = p.Decode2();
						e.VX = p.Decode2();
						e.VY = p.Decode2();
						Foothold = p.Decode2();
						e.fhLast = Foothold;
						e.FH = Foothold;
						if (e.nAttr == MPA_FALLDOWN)
							e.fhFallStart = p.Decode2();

						e.xOffset = p.Decode2();
						e.yOffset = p.Decode2();
						break;
					case MPA_JUMP:
					case MPA_IMPACT:
					case MPA_STARTWINGS:
					case MPA_MOB_TOSS:
					case MPA_DASH_SLIDE:
					case MPA_MOB_LADDER:
					case MPA_MOB_RIGHTANGLE:
					case MPA_MOB_STOPNODE_START:
					case MPA_MOB_BEFORE_NODE:
						e.X = oldX;
						e.Y = oldY;
						e.VX = p.Decode2();
						e.VY = p.Decode2();
						break;
					case MPA_IMMEDIATE:
					case MPA_TELEPORT:
					case MPA_ASSAULTER:
					case MPA_ASSASSINATION:
					case MPA_RUSH:
					case MPA_SITDOWN:
						e.X = p.Decode2();
						e.Y = p.Decode2();
						Foothold = p.Decode2();
						e.fhLast = Foothold;
						e.FH = Foothold;
						break;
					case MPA_STATCHANGE:
						e.bStat = p.Decode1();
						e.X = oldX;
						e.Y = oldY;
						Foothold = 0;
						break;
					case MPA_STARTFALLDOWN:
						e.X = oldX;
						e.Y = oldY;
						e.VX = p.Decode2();
						e.VY = p.Decode2();
						e.fhFallStart = p.Decode2();
						break;
					case MPA_FLYING_BLOCK:
						e.X = p.Decode2();
						e.Y = p.Decode2();
						e.VX = p.Decode2();
						e.VY = p.Decode2();
						break;
					case MPA_FLASHJUMP:
					case MPA_ROCKET_BOOSTER:
					case MPA_BACKSTEP_SHOT:
					case MPA_MOBPOWERKNOCKBACK:
					case MPA_VERTICALJUMP:
					case MPA_CUSTOMIMPACT:
					case MPA_COMBATSTEP:
					case MPA_HIT:
					case MPA_TIMEBOMBATTACK:
					case MPA_SNOWBALLTOUCH:
					case MPA_BUFFZONEEFFECT:
						e.X = oldX;
						e.Y = oldY;
						e.VX = oldVX;
						e.VY = oldVY;
						break;
					default:
						// there shouldnt be a default case -- all cases are covered. most likely PE
						break;
				}

				if (e.nAttr != MPA_STATCHANGE)
				{
					e.bMoveAction = p.Decode1();
					e.tElapse = p.Decode2();

					oldX = e.X;
					oldY = e.Y;
					oldVX = e.VX;
					oldVY = e.VY;
				}

				elements[i] = e;
			}

			if (false) //bPassive -- CUser::OnPassiveMove
			{
				var nKeyPadState = p.Decode1();

				//KeyPadState are 4 bits each

				for (int i = 0; i < nKeyPadState / 2; i++)
				{
					/*aKeyPadState[i] = */
					p.Decode1();
				}

				//rcMove 
				var rcMoveLeft = p.Decode2();
				var rcMoveTop = p.Decode2();
				var rcMoveRight = p.Decode2();
				var rcMoveBottom = p.Decode2();
			}

			if (elements.Length > 0)
			{
				var e = elements[elements.Length - 1];
				X = e.X;
				Y = e.Y;
				MoveAction = (byte)(e.bMoveAction & 0xFF); // TODO validate this
				Foothold = e.FH;
			}

			return elements;
		}

		public CMovePath Clone()
		{
			return new CMovePath
			{
				X = this.X,
				Y = this.Y,
				VX = this.VX,
				VY = this.VY,
				Foothold = this.Foothold,
				MoveAction = this.MoveAction
			};
		}

		public override string ToString()
		{
			return $"Position: {X},{Y} - Fh: {Foothold} - Stance: {MoveAction}";
		}
	}
}
