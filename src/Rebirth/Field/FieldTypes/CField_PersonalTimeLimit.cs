using Rebirth.Characters;
using Rebirth.Server.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Field.FieldTypes
{
	public class CField_PersonalTimeLimit : CField
	{
		public CField_PersonalTimeLimit(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId) { }

		protected override void Init()
		{
			CreateFieldClock(Template.TimeLimit);
			base.Init();
		}

		protected override void OnClockEnd()
		{
			foreach (var user in new List<Character>(Users))
			{
				user.Action.SetField(Template.ForcedReturn);
			}

			base.OnClockEnd();
		}
	}
}
