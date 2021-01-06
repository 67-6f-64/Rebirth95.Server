using log4net;
using Rebirth.Entities.Item;
using Rebirth.Game;
using Rebirth.Network;
using System;
using Rebirth.Common.Tools;
using Rebirth.Entities;
using Rebirth.Tools;

namespace Rebirth.Field.FieldObjects
{
	/*
    struct __declspec(align(8)) DROP
    {
      SECPOINT pt1;
      __POSITION *posList;
      char nEnterType;
      char bReal;
      unsigned int dwId;
      unsigned int dwOwnerID;
      unsigned int dwSourceID;
      int nOwnType;
      int bIsMoney;
      int nInfo;
      _com_ptr_t<_com_IIID<IWzGr2DLayer,&_GUID_6dc8c7ce_8e81_4420_b4f6_4b60b7d5fcdf> > pLayer;
      _com_ptr_t<_com_IIID<IWzVector2D,&_GUID_f28bd1ed_3deb_4f92_9eec_10ef5a1c3fb4> > pvec;
      int tCreateTime;
      int tLeaveTime;
      int nState;
      int tTickTime;
      int tEndParabolicMotion;
      int tLastTryPickUp;
      unsigned int dwPickupID;
      long double fAngle;
      SECPOINT pt2;
      _FILETIME m_dateExpire;
      int bByPet;
    };
    */
	public class CDrop : CFieldObj
	{
		public static ILog Log = LogManager.GetLogger(typeof(CDrop));

		/// <summary>
		/// Owner ID of drop. DropType must be set to UserOwn in order for this to be encoded.
		/// Only one owner type/ID can be encoded per drop.
		/// </summary>
		public int OwnerCharId { get; set; }
		/// <summary>
		/// Party ID of the drop. DropType must be set to PartyOwn in order for this to be encoded.
		/// Only one owner type/ID can be encoded per drop.
		/// </summary>
		public int OwnerPartyId { get; set; }
		public int SourceId { get; set; }

		public GW_ItemSlotBase Item { get; set; }
		public int ItemId { get; set; }

		public DateTime SpawnTime { get; set; }
		public short DropExpirySeconds { get; set; }

		public DropEnterType nEnterType { get; set; }
		public DropLeaveType nLeaveType { get; set; }

		/// <summary>
		/// This indicates the pet slot when drop is picked up by a pet.
		/// </summary>
		public byte PetIdx { get; set; }

		public byte bIsMoney { get; set; } // // 1 mesos, 0 item, 2 and above all item meso bag,
		public int nMesoVal => ItemId;
		public short tDelay { get; set; }

		public short StartPosX { get; }
		public short StartPosY { get; }

		public DropOwnType DropOwnType { get; set; } = DropOwnType.NoOwn;

		public string QR { get; set; } = "";

		public int DropOwnerID
			=> DropOwnType == DropOwnType.PartyOwn
			? OwnerPartyId
			: DropOwnType == DropOwnType.UserOwn
			? OwnerCharId
			: 0;

		/// <summary>
		/// Make sure to clone the movepath when passing it.
		/// </summary>
		/// <param name="pStartPos"></param>
		/// <param name="ownerId"></param>
		/// <param name="enterType"></param>
		public CDrop(CMovePath pStartPos, int ownerId)
			: this(pStartPos.CurrentXY, ownerId) { }

		public override void Dispose()
		{
			Item = null;
			base.Dispose();
		}

		public CDrop(TagPoint tp, int ownerId)
		{
			OwnerCharId = ownerId;

			SpawnTime = DateTime.Now;

			StartPosX = tp.X;
			StartPosY = tp.Y;

			nLeaveType = DropLeaveType.TimeOut;
			nEnterType = DropEnterType.Create;
		}

		/// <summary>
		/// Sets the target Y pos to the foothold below the Position.X and StartPosY values
		/// </summary>
		/// <param name="field">Field that the drop will calculated the foothold in</param>
		public void CalculateY(CField field, short nDefaultY)
		{
			Foothold fh = null;

			for (var i = 100; i <= 1000; i += 100)
			{
				fh = field.Footholds.FindBelow(Position.X, (short)(StartPosY - i));

				if (fh != null) break;
			}

			if (fh is null)
			{
				Position.Y = nDefaultY;
			}
			else
			{
				Position.Y = fh.Y1;
			}
		}

		public override COutPacket MakeEnterFieldPacket() => CPacket.DropEnterField(this);
		public override COutPacket MakeLeaveFieldPacket() => CPacket.DropLeaveField(this);
	}

	public enum DropOwnType : byte
	{
		UserOwn = 0,
		PartyOwn = 1,
		NoOwn = 2,
		Explosive_NoOwn = 3,
	}

	public enum DropLeaveType : byte
	{
		TimeOut = 0,
		ScreenScroll = 1,
		UserPickup = 2,
		MobPickup = 3,
		Explode = 4,
		PetPickup = 5,
		PassConvex = 6, // ??
		PetSkill = 7,
	}

	/// <summary>
	/// These are from vertisy, I'm still trying to figure out the Nexon names for these
	/// </summary>
	public enum DropEnterType : byte
	{
		None = 0,
		Create = 1,
		OnFoothold = 2,
		FadingOut = 3,
	}
}
