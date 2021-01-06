using Rebirth.Entities;
using System;
using System.Collections.Generic;
using Rebirth.Common.Tools;
using Rebirth.Provider.Template.Map;
using Rebirth.Tools;

namespace Rebirth.Field
{
	public class CFootholdMap
	{
		// splitting these should make fh calculations lighter
		private readonly List<Foothold> _wallFH = new List<Foothold>(); // ~37% of footholds

		private readonly List<Foothold> _nonWallFH = new List<Foothold>(); // ~46% of footholds

		public List<Foothold> NonWallFHs => _nonWallFH;

		public int CenterPointX { get; private set; }

		public int MapArea { get; private set; }

		public void Dispose()
		{
			_wallFH.Clear();
			_nonWallFH.Clear();
		}

		public int Count => _nonWallFH.Count;

		public List<Foothold> GetFootholdRandom(int nCount, TagRect rcRange)
		{
			var ret = new List<Foothold>();
			// IntersectRect(&rcArea, rcRange, &v4->m_rcMBR); ???

			var nStart = rcRange.Left;
			var nGrid = (rcRange.Right - rcRange.Left + 1) / (2 * nCount);

			// uhh this is too complicated

			return ret;
		}

		// CWvsPhysicalSpace2D::GetFootholdRange(CWvsPhysicalSpace2D *this, int x, int y1, int y2, ZList<long> *lPosition)
		public List<Foothold> GetFootholdRange(int ptX, int ptY1, int ptY2)
		{
			var ret = new List<Foothold>();

			foreach (var fh in _nonWallFH)
			{
				if (fh.X1 < fh.X2 && fh.X1 <= ptX && fh.X2 >= ptX)
				{
					var v11 = fh.Y1 + (ptX - fh.X1) * (fh.Y2 - fh.Y1) / (fh.X2 - fh.X1);
					if (v11 >= ptY1 && ptY2 >= v11)
					{
						ret.Add(fh);
					}
				}
			}

			return ret;
		}

		public void Load(MapTemplate template)
		{
			var left = 0;
			var right = 0;
			var bottom = 0;
			var top = 0;

			foreach (var f in template.Footholds)
			{
				var entry = new Foothold
				{
					Id = (short)f.TemplateId,
					Next = f.Next,
					Prev = f.Prev,
					X1 = f.X1,
					X2 = f.X2,
					Y1 = f.Y1,
					Y2 = f.Y2
				};

				bottom = Math.Max(bottom, f.Y2);
				top = Math.Min(top, f.Y1);

				left = Math.Min(left, f.X1); // x1 is always smaller
				right = Math.Max(right, f.X2); // x2 is always bigger

				if (entry.Wall)
				{
					_wallFH.Add(entry);
				}
				else
				{
					_nonWallFH.Add(entry);
				}
			}

			// offsetting the the fact that we are using the top-most foothold (ie not the real top)
			top -= 250;

			var length = MathHelper.SlopeDistance(left, 0, right, 0);
			var height = MathHelper.SlopeDistance(0, bottom, 0, top);

			MapArea = (int)(length * height);
			CenterPointX = (short)(((right - left) / 2) + left);
		}

		public Foothold FindBelow(short pX, short pY)
		{
			List<Foothold> xMatches = new List<Foothold>();

			foreach (Foothold fh in NonWallFHs) // find fhs with matching x coordinates
			{
				if (fh.X1 <= pX && fh.X2 >= pX)
					xMatches.Add(fh);
			}

			xMatches.Sort();

			foreach (Foothold fh in xMatches)
			{
				if (!fh.Wall && fh.Y1 != fh.Y2)
				{
					int calcY;
					double s1 = Math.Abs(fh.Y2 - fh.Y1);
					double s2 = Math.Abs(fh.X2 - fh.X1);
					double s4 = Math.Abs(pX - fh.X1);
					double alpha = Math.Atan(s2 / s1);
					double beta = Math.Atan(s1 / s2);
					double s5 = Math.Cos(alpha) * (s4 / Math.Cos(beta));

					if (fh.Y2 < fh.Y1)
					{
						calcY = fh.Y1 - ((int)s5);
					}
					else
					{
						calcY = fh.Y1 + ((int)s5);
					}

					if (calcY >= pY)
					{
						return fh;
					}
				}
				else if (!fh.Wall && fh.Y1 >= pY)
				{
					return fh;
				}
			}
			return null;
		}
	}
}
