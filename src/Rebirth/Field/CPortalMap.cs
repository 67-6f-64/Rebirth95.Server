using Rebirth.Field.FieldObjects;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.Map;
using Rebirth.Tools;

namespace Rebirth.Field
{
	public class CPortalMap
	{
		private readonly List<CPortal> Portals = new List<CPortal>();

		public void Dispose() => Portals.Clear();

		public CPortal GetPortalInRect(int pX, int pY, int rectSize)
		{
			foreach (var portal in Portals)
			{

				// assuming pX and pY are the center of the rect
				var r = rectSize / 2;

				// x is normal (low value left, high value right)
				var lpX = pX - r; // left point x
				var rpX = pX + r; // right point x

				// y range is inverted (low value top, high value bot)
				var topY = pY - r; // top point y
				var botY = pY + r; // bottom point y

				if ((portal.nX > lpX) && (portal.nX < rpX) && (portal.nY > topY) && (portal.nY < botY))
				{
					return portal;
				}
			}

			return null; // unable to find portal in rect
		}

		public void Load(MapTemplate template)
		{
			foreach (var portal in template.Portals)
			{
				var entry = new CPortal()
				{
					nIdx = portal.nIndex,
					nType = (PortalType)portal.nPortalType,
					nX = portal.nX,
					nY = portal.nY,
					sName = portal.sPortalName,
					sTName = portal.sTargetName,
					nTMap = portal.nTargetMap
				};
				Portals.Add(entry);
			}
		}

		//int __thiscall CPortalMap::EnablePortal(CPortalMap *this, const char *sName, int bEnable)
		//CPortal *__thiscall CPortalMap::FindCloseStartPoint(CPortalMap *this, int x, int y)
		//CPortal *__thiscall CPortalMap::FindPortal(CPortalMap *this, const char *sName)
		//CPortal *__thiscall CPortalMap::GetRandStartPoint(CPortalMap *this)
		//CPortal *__thiscall CPortalMap::GetRandStartPoint2(CPortalMap *this)
		//CPortal *__thiscall CPortalMap::GetRandStartPoint3(CPortalMap *this)
		//int __thiscall CPortalMap::IsPortalNear(CPortalMap *this, ZArray<tagPOINT> *aptRoute, int nXrange)
		//void __thiscall CPortalMap::ResetPortal(CPortalMap *this)
		//void __thiscall CPortalMap::RestorePortal(CPortalMap *this, CField *pField, _com_ptr_t<_com_IIID<IWzProperty,&_GUID_986515d9_0a0b_4929_8b4f_718682177b92> > pPropPortal)

		public CPortal FindPortal(string name) => Portals.FirstOrDefault(p => p.sName == name);
		public CPortal FindPortal(int id) => Portals.FirstOrDefault(p => p.nIdx == id);

		public CPortal GetRandStartPoint()
		{
			var list = Portals.Where(p => p.sName == "sp").ToArray();

			if (list.Length == 0)
				return null;

			return list.Random();
		}
	}
}
