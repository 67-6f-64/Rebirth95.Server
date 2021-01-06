using Rebirth.Network;

namespace Rebirth.Common.Tools
{
	public class TagRect
	{
		public int Left { get; set; }
		public int Top { get; set; }
		public int Right { get; set; }
		public int Bottom { get; set; }

		public bool IsEmpty =>
			Left == Right &&
			Top == Bottom;

		public TagRect() { }

		public TagRect(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public void OffsetRect(TagPoint pt, bool bPositive = true) => OffsetRect(pt.X, pt.Y, bPositive);
		public void OffsetRect(int ptX, int ptY, bool bPositive = true)
		{
			Top += ptY;
			Bottom += ptY;

			Left += ptX;
			Right += ptX;
		}

		/// <summary>
		/// Determines if the position is within the bounds of this object
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public bool PointInRect(TagPoint pt) => PointInRect(pt.X, pt.Y);
		public bool PointInRect(int ptX, int ptY) => Left <= ptX && ptX <= Right && Top <= ptY && ptY <= Bottom;

		public void Encode(COutPacket p)
		{
			p.Encode4(Left);
			p.Encode4(Top);
			p.Encode4(Right);
			p.Encode4(Bottom);
		}

		public TagRect Copy() => new TagRect(Left, Top, Right, Bottom);

		public override string ToString()
		{
			return $"[L {Left}] [T {Top}] [R {Right}] [B {Bottom}]";
		}
	}
}