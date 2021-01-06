using Autofac;
using log4net;
using Rebirth.Characters.Modify;
using Rebirth.Client;
using Rebirth.Common.Types;
using Rebirth.Network;
using Rebirth.Redis;
using Rebirth.Server.Center;
using Rebirth.Server.Game;

namespace Rebirth.Characters.Actions
{
	public class MigrationAction
	{
		public static ILog Log = LogManager.GetLogger(typeof(MigrationAction));

		public static void ITC(WvsGameClient c, CInPacket p)
		{
			if (c.Character.Stats.nHP <= 0) return;
			if (c.Character.Field.Template.HasMigrateLimit()) return;
			if (c.Character.Field.IsInstanced) return;

			var Parent = c.Character;
			if (Parent.Stats.nLevel < 16)
			{
				Parent.SendPacket(CPacket.TransferChannelReqIgnore(5));
			}
			else
			{
				//Real Logic Here : Disabled it for now
				Parent.SendPacket(CPacket.TransferChannelReqIgnore(3));
			}
		}

		public static void CashShop(WvsGameClient c, CInPacket p)
		{
			if (c.Character.Stats.nHP <= 0) return;
			if (c.Character.Field.Template.HasMigrateLimit()) return;
			if (c.Character.Field.IsInstanced) return;

			var Parent = c.Character;
			Parent.Socket.Account.Save();
			Parent.Stats.Channel = Parent.Socket.ChannelId;
			Parent.Save();

			var center = ServerApp.Container.Resolve<WvsCenter>();

			var nPort = (short)center.WvsShop.Port;

			var storage = ServerApp.Container.Resolve<CenterStorage>();
			storage.AddCharacterMigrate(Parent.dwId);
			storage.AddCharacterCSITC(Parent.dwId);

			Parent.NotifySocialChannels(SocialNotiflag.MigrateCashShop);

			Parent.SendPacket(CPacket.MigrateCommand(Constants.ServerAddress, nPort));
		}

		/**
         * Die packet:
         * Recv [CP_UserTransferFieldRequest] [29 00] [01] [00 00 00 00] 00 00 00 01 00 // last bytes possibly related to death-items
         * Portal to other map packet:
         * Recv [CP_UserTransferFieldRequest] [29 00] [01] [FF FF FF FF] [06 00] [65 61 73 74 30 30] [4D 06] [C7 01] 00 00 00
         */
		public static void Field(WvsGameClient c, CInPacket p)
		{
			//void __thiscall CUser::OnTransferFieldRequest(CUser *this, int bLoopback, CInPacket *iPacket)

			//Exit Cash Shop -> Not used in WvsGame
			if (p.Available == 0) return;

			//TODO: Portal count checks
			//TODO: XY rect checks to ensure player is on portal when activating it
			//TODO: Keep track if player spawns when entering a field

			var bFieldKey = p.Decode1(); //CField::GetFieldKey(v20);
			var dwField = p.Decode4();
			var sPortalName = p.DecodeString();

			if (c.Character.Stats.nHP <= 0)
			{
				c.Character.Field.OnUserWarpDie(c.Character);
			}
			else if (sPortalName.Length > 0)    // not death
			{
				var x = p.Decode2();
				var y = p.Decode2();

				p.Decode1(); // used to be bTownPortal
				var bPremium = p.Decode1();
				var bChase = p.Decode1();

				if (bChase > 0)
				{
					var nTargetPosition_X = p.Decode4();
					var nTargetPosition_Y = p.Decode4();
				}

				c.Character.Field.OnUserEnterPortal(c.Character, sPortalName);
			}
			else // gm warp command
			{
				// TODO admin checks
				// tp to desired field
			}
		}

		public static void Channel(WvsGameClient c, CInPacket p)
		{
			if (c.Character.Stats.nHP <= 0) return;
			if (c.Character.Field.Template.HasMigrateLimit()) return;
			if (c.Character.Field.IsInstanced) return;

			var center = ServerApp.Container.Resolve<WvsCenter>();
			var Parent = c.Character;
			var nChannel = p.Decode1();

			if (nChannel >= center.WvsGames.Length)
			{
				Parent.SendPacket(CPacket.TransferChannelReqIgnore(1));
			}
			else
			{
				var nPort = (short)center.WvsGames[nChannel].Port;

				var storage = ServerApp.Container.Resolve<CenterStorage>();
				storage.AddCharacterMigrate(Parent.dwId);

				Parent.Stats.Channel = center.WvsGames[nChannel].ChannelId;

				Parent.Socket.Account.Save();
				Parent.Save();

				Parent.NotifySocialChannels(SocialNotiflag.ChangeChannel);

				Parent.SendPacket(CPacket.MigrateCommand(Constants.ServerAddress, nPort));
			}
		}
	}
}
