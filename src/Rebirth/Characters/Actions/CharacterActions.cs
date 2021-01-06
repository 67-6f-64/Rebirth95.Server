using Rebirth.Characters.Actions;
using Rebirth.Network;
using Rebirth.Server.Center;
using System.Linq;

namespace Rebirth.Characters
{
	public class CharacterActions
	{
		public Character Parent => MasterManager.CharacterPool.Get(dwParentID);
		public int dwParentID { get; set; }
		public ConsumeItemAction ConsumeItem { get; private set; }

		public CharacterActions(int c)
		{
			ConsumeItem = new ConsumeItemAction(c);
			dwParentID = c;
		}

		public void Dispose()
		{
			ConsumeItem = null;
		}

		public void Enable() => Parent.SendPacket(CPacket.StatChanged(true)); //ExclRequest Reset

		public void SetFieldInstance(int nMapId, int nInstanceId, byte nPortal = 0, short nFh = 0)
			=> SetField(nMapId, nInstanceId, nPortal, nFh);
		public void SetField(int nMapId, byte nPortal = 0, short nFh = 0)
			=> SetField(nMapId, 0, nPortal, nFh);

		private void SetField(int nMapId, int nInstanceId, byte nPortal, short nFh)
		{
			var sock = Parent.Socket;
			var newField = sock.Server.CFieldMan.GetField(nMapId, nInstanceId);

			if (newField == null) return;

			Parent.Field?.OnUserLeave(Parent);

			Parent.Stats.dwPosMap = nMapId;
			Parent.Stats.nPortal = nPortal;
			Parent.Position.Foothold = nFh;

			Parent.Position.MoveAction = 2; // MoveActionType.STAND

			var portal = newField.Portals.FindPortal(nPortal) ?? newField.Portals.FindPortal(0);

			Parent.Position.X = portal.nX;
			Parent.Position.Y = portal.nY;

			newField.AddClient(sock);
		}

		public void FeatureNotAddedMessage(string type) //Just for Alpha / Beta
		{
			SystemMessage("Feature [{0}] has not been implemented yet.", type);
			Enable();
		}

		public void SetADBoard(string msg)
		{
			if (msg != null && msg.Length <= Constants.MaxAdBoardLength)
			{
				Parent.sADBoard = msg;
				Parent.Field.Broadcast(CPacket.UserADBoard(Parent.dwId, msg));
			}
			else
			{
				Parent.sADBoard = "";
				Parent.Field.Broadcast(CPacket.UserADBoard(Parent.dwId));
			}
		}

		public void SystemMessage(string fmt, params object[] args)
		{
			var message = string.Format(fmt, args);
			Parent.SendPacket(CPacket.SystemMessage(message));
		}

		public void BoxMessage(string fmt, params object[] args)
		{
			var message = string.Format(fmt, args);
			Parent.SendPacket(CPacket.WarnMessage(message));
		}

		public void HackMessage(string fmt, params object[] args)
		{
			var message = string.Format(fmt, args);
			Parent.SendPacket(CPacket.DataCRCCheckFailed(message));
		}

		public void OpenGate(CInPacket p)
		{
			var field = Parent.Field;

			var dwOwnerID = p.Decode4();
			var nX = p.Decode2();
			var nY = p.Decode2();
			var bFirst = p.Decode1() != 0;

			(bFirst ? field.OpenGates1 : field.OpenGates2)
				.OnOpenGate(Parent, dwOwnerID);

			Enable();
		}
	}
}
