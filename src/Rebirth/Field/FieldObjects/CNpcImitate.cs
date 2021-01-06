using Rebirth.Entities;
using Rebirth.Network;

namespace Rebirth.Field.FieldObjects
{
	public class CNpcImitate : CNpc
	{
		//Must use Npc Template 9901000 - 9901919

		public string Name { get; set; }
		public AvatarLook Look { get; set; }

		public CNpcImitate(int nTemplateID) : base(nTemplateID)
		{

		}

		public void EncodeImitateData(COutPacket p)
		{
			p.Encode4(TemplateId);
			p.EncodeString(Name);
			Look.Encode(p);
		}
	}
}
