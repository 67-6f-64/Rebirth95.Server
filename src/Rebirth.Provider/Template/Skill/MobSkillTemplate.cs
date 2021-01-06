using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Rebirth.Provider.Template.Skill
{
	public sealed class MobSkillTemplate : AbstractTemplate
	{
		[JsonProperty]
		private readonly Dictionary<int, MobSkillLevelData> _levelData;

		public MobSkillLevelData this[int key]
			=> _levelData.ContainsKey(key)
				? _levelData[key]
				: null;

		public MobSkillLevelData GetLevelData(int nLevel)
			=> this[nLevel];

		public void InsertLevelData(MobSkillLevelData pData)
			=> _levelData.Add(pData.nSLV, pData);

		public MobSkillTemplate(int templateId) : base(templateId)
		{
			_levelData = new Dictionary<int, MobSkillLevelData>();
		}
	}
}
