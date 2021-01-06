using Rebirth.Characters.Skill;
using Rebirth.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Common.Types;
using Rebirth.Common.GameLogic;

namespace Rebirth.Characters.Modify
{
	public class SkillModifier
	{
		public bool NeedsStatRecalc { get; private set; }
		public bool NeedsRegenRecalc { get; private set; }

		public int Count => Entries.Count;

		private readonly List<SkillEntry> Entries;
		private Character Parent; // we don't want anyone poking at this

		public SkillModifier(Character c)
		{
			Parent = c;
			Entries = new List<SkillEntry>();
		}

		public void AddEntry(int nSkillID, Action<SkillEntry> action)
		{
			var skillEntry = Parent.Skills.FirstOrDefault(s => s.nSkillID == nSkillID) ?? new SkillEntry(nSkillID);

			var prevSLV = skillEntry.nSLV;

			action.Invoke(skillEntry);

			if (prevSLV != skillEntry.nSLV)
			{
				if (skillEntry.nSLV - skillEntry.CombatOrders > skillEntry.MaxLevel)
				{
					skillEntry.nSLV = skillEntry.MaxLevel;
				}

				if (skillEntry.Template.PsdSkill > 0 || skillEntry.Template.AdditionPsd.Length > 0)
				{
					NeedsStatRecalc = true;
				}

				switch ((Skills)nSkillID)
				{
					case Skills.CRUSADER_UPGRADE_MP_RECOVERY:
					case Skills.SOULMASTER_UPGRADE_MP_RECOVERY:
						NeedsRegenRecalc = true;
						break;
				}
			}

			// we don't notify the client about these
			foreach (var nLinkSkillID in SkillLogic.GetHiddenLinkSkills(nSkillID))
			{
				var linkSkillEntry = Parent.Skills[nLinkSkillID] ?? new SkillEntry(nLinkSkillID);

				linkSkillEntry.nSLV = skillEntry.nSLV;
				linkSkillEntry.CurMastery = skillEntry.CurMastery;

				if (!Parent.Skills.Contains(nLinkSkillID))
				{
					Parent.Skills.Add(linkSkillEntry);
				}
			}

			if (!Parent.Skills.Contains(skillEntry.nSkillID))
			{
				Parent.Skills.Add(skillEntry);
			}

			if (!skillEntry.Template.IsHiddenSkill)
			{
				Entries.Add(skillEntry);
			}
		}

		public void Encode(COutPacket p)
		{
			p.Encode2((short)Entries.Count);

			foreach (var entry in Entries)
			{
				entry.Encode(p);
			}
		}
	}
}
