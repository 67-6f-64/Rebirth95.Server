using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Network;

namespace Rebirth.Characters.Skill.ActiveSkill
{
	public class ActiveSkill_OpenGate
    {
        public static void Handle(int nSkillID, byte nSLV, Character c, CInPacket p)
        {
            //COutPacket::COutPacket(&oPacket, 103);
            //v30 = 1;
            //v21 = get_update_time();
            //COutPacket::Encode4(&oPacket, v21);
            //COutPacket::Encode4(&oPacket, pSkill->nSkillID);
            //COutPacket::Encode1(&oPacket, v16);
            //v22 = (unsigned __int16 *)(*(int(__thiscall * *)(int, char *))(*(_DWORD*)v10 + 16))(v10, &v28);
            //COutPacket::Encode2(&oPacket, *v22);
            //v23 = (*(int(__thiscall * *)(int, CUserLocal * *))(*(_DWORD*)v10 + 16))(v10, &v24);
            //COutPacket::Encode2(&oPacket, *(_WORD*)(v23 + 4));
            //CClientSocket::SendPacket(TSingleton < CClientSocket >::ms_pInstance, &oPacket);

            var x = p.Decode2();
            var y = p.Decode2();

            var bLeft = x < c.Position.X;

            if (c.Skills.Cast(nSkillID, bLeft, true)) // todo handle this skill
            {
				var cGate = new COpenGate(c.dwId)
				{
					Position = new CMovePath { X = x, Y = y } // TODO distance check 
                };

                var gate1 = c.Field.OpenGates1[c.dwId];
                var gate2 = c.Field.OpenGates2[c.dwId];

                if (gate1 is null)
                {
                    c.Field.OpenGates1.Add(cGate);
                }
                else if (gate2 is null)
                {
                    c.Field.OpenGates2.Add(cGate);
                }
                else if (gate1.StartTime > gate2.StartTime)
                {
                    c.Field.OpenGates2.Remove(cGate.dwCharacterID);
                    c.Field.OpenGates2.Add(cGate);
                }
                else
                {
                    c.Field.OpenGates1.Remove(cGate.dwCharacterID);
                    c.Field.OpenGates1.Add(cGate);
                }

				new UserEffectPacket(Common.Types.UserEffect.SkillUse)
				{
					nSkillID = nSkillID,
					nSLV = nSLV,
					bLeft = (c.Position.MoveAction & 1) != 0
				}.BroadcastEffect(c, false);
			}
		}
    }
}
