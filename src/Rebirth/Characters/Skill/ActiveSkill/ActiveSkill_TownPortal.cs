using Rebirth.Game;
using Rebirth.Network;
using System;

namespace Rebirth.Characters.Skill.ActiveSkill
{
	public class ActiveSkill_TownPortal
	{
		public static void Handle(int nSkillID, byte nSLV, Character c, CInPacket p)
		{
			//      COutPacket::COutPacket(&oPacket, 103);
			//v36 = 2;
			//v25 = get_update_time();
			//      COutPacket::Encode4(&oPacket, v25);
			//COutPacket::Encode4(&oPacket, pSkill->nSkillID);
			//COutPacket::Encode1(&oPacket, v20);
			//v26 = (unsigned __int16 *)(*(int (__thiscall**) (int, char*))(*(_DWORD*) v15 + 16))(v15, &v34);
			//COutPacket::Encode2(&oPacket, *v26);
			//v27 = (*(int (__thiscall**) (int, CUserLocal**))(*(_DWORD*) v15 + 16))(v15, &v30);
			//COutPacket::Encode2(&oPacket, *(_WORD*) (v27 + 4));
			//CClientSocket::SendPacket(TSingleton<CClientSocket>::ms_pInstance, &oPacket);

			var x = p.Decode2();
			var y = p.Decode2();

			if (c.Field.Template.Town) return;
			if (c.Field.MapId / 1000000 % 100 == 9) return;
			if (c.Field.Template.HasMysticDoorLimit()) return;

			return; // TODO fix this fucking shit

			if (!c.Skills.Cast(nSkillID, true, true)) return;

			c.Field.TownPortals.CreateTownPortal(c, nSkillID, x, y, Math.Min(120 * 1000, 20 * nSLV * 1000)); // TODO get time value from skill

			c.Party?.BroadcastLoadParty(); // TODO send the proper packet instead of this whole thing

			new UserEffectPacket(Common.Types.UserEffect.SkillUse)
			{
				nSkillID = nSkillID,
				nSLV = nSLV,
				bLeft = (c.Position.MoveAction & 1) != 0
			}.BroadcastEffect(c, false);
		}
	}
}
