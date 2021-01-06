using Rebirth.Common.Types;

namespace Rebirth.Field.FieldObjects
{
	public sealed class CPortal
	{
		public int nIdx { get; set; }
		public string sName { get; set; }
		public PortalType nType { get; set; }
		public short nX { get; set; }
		public short nY { get; set; }
		public int nTMap { get; set; }
		public string sTName { get; set; }
		//public int bHideTooltip;
		//public int bOnlyOnce;
		//public int nDelayTime;
		//public string sImage;
		//public int nVImpact;
		//public int nHImpact;
		//public string sReactorName;
		//public string sSessionValueKey;
		//public string sSessionValue;

		public override string ToString()
		{
			return $"{nIdx} @ ( {nX}, {nY} ) -> {nTMap}";
		}
	}

	//    struct __cppobj CPortal : CGameObject
	//{
	//  unsigned int dwFieldID;
	//    int nIdx;
	//    ZXString<char> sName;
	//    int bEnable;
	//    int nType;
	//    tagPOINT ptPos;
	//    int nTMap;
	//    ZXString<char> sTName;
	//    ZXString<char> sScript;
	//};



	//public sealed class Portal
	//{
	//    public byte ID { get; set; }
	//    public string Name { get; set; }
	//    public PortalType Type { get; set; }
	//    public int Destination { get; set; }
	//    public string DestinationName { get; set; }
	//    public short X { get; set; }
	//    public short Y { get; set; }

	//    //public Point ToPoint()
	//    //{
	//    //    return new Point(X, Y);
	//    //}

	//    public Portal() { }

	//    public Portal(short x, short y)
	//    {
	//        X = x;
	//        Y = y;
	//    }

	//    public bool IsGoodPortal()
	//    {
	//        if (Destination == ID)
	//            return false;

	//        if (Destination == 999999999)
	//            return false;

	//        switch (Type)
	//        {
	//            case PortalType.INVISIBLE:
	//            case PortalType.NORMAL:
	//            case PortalType.NORMAL_ALT:
	//            case PortalType.HIDDEN:
	//            case PortalType.HIDDEN_ALT:
	//                break;
	//            default:
	//                return false;
	//        }

	//        return true;
	//    }

	//    public override string ToString()
	//    {
	//        return $"#{ID} @ {X}.{Y} -> {Destination}";
	//    }
	//}

	//[4:59:12 PM] Neal: zythgor3 actually found that shit
	//[4:59:17 PM] Neal: THEY ALL MADE FUN OF HIM FOR IT
	//[4:59:23 PM] Neal: BUT HE SHOWED THEM

	//LOL @ ABOVE xD

	//public enum PortalType : int
	//{
	//    //see zythgor3's CEF post: http://snsgaming.com/cef/viewtopic.php?p=2616482&highlight=#2616482
	//    PLACEHOLDER = 0, //no image, no use
	//    INVISIBLE, //no image, can use
	//    NORMAL, //has image, can use
	//    AUTOMATIC, //no image, automatically use
	//    STOCK_BLOCKING, //has image, no use with block
	//    USELESS_BLOCKING, //no image, no use with block
	//    USELESS, //no image, no use
	//    NORMAL_ALT, //has image, can enter
	//    INVISIBLE_SCRIPTED, //no image, can use with script
	//    INVISIBLE_BLOCKING, //no image, can use with block
	//    HIDDEN, //no image, can enter
	//    HIDDEN_ALT //no image, can enter
	//}
}
