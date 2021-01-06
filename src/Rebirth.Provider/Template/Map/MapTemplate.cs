using Rebirth.Provider.Exception;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Common.Types;

namespace Rebirth.Provider.Template.Map
{
	public sealed class MapTemplate : AbstractTemplate
	{
		private int _forcedReturn;
		public int ForcedReturn
		{
			get => _forcedReturn;
			set
			{
				if (!Locked) _forcedReturn = value;
				else throw new TemplateAccessException();
			}
		}

		private int _returnmap;
		public int ReturnMap
		{
			get => _returnmap;
			set
			{
				if (!Locked) _returnmap = value;
				else throw new TemplateAccessException();
			}
		}

		private bool _flymap;
		public bool FlyMap
		{
			get => _flymap;
			set
			{
				if (!Locked) _flymap = value;
				else throw new TemplateAccessException();
			}
		}

		private bool _town;
		public bool Town
		{
			get => _town;
			set
			{
				if (!Locked) _town = value;
				else throw new TemplateAccessException();
			}
		}

		private string _onfirstuserenter;
		public string OnFirstUserEnter
		{
			get => _onfirstuserenter;
			set
			{
				if (!Locked) _onfirstuserenter = value;
				else throw new TemplateAccessException();
			}
		}

		private string _onuserenter;
		public string OnUserEnter
		{
			get => _onuserenter;
			set
			{
				if (!Locked) _onuserenter = value;
				else throw new TemplateAccessException();
			}
		}

		private FieldType _fieldtype;
		public FieldType FieldType
		{
			get => _fieldtype;
			set
			{
				if (!Locked) _fieldtype = value;
				else throw new TemplateAccessException();
			}
		}

		private int _dechp;
		public int DecHP
		{
			get => _dechp;
			set
			{
				if (!Locked) _dechp = value;
				else throw new TemplateAccessException();
			}
		}

		private int _decmp;
		public int DecMP
		{
			get => _decmp;
			set
			{
				if (!Locked) _decmp = value;
				else throw new TemplateAccessException();
			}
		}

		private int _decinterval;
		public int DecInterval
		{
			get => _decinterval;
			set
			{
				if (!Locked) _decinterval = value;
				else throw new TemplateAccessException();
			}
		}

		private int _protectitem;
		public int ProtectItem
		{
			get => _protectitem;
			set
			{
				if (!Locked) _protectitem = value;
				else throw new TemplateAccessException();
			}
		}

		private FieldOpt _fieldlimit;
		public FieldOpt FieldLimit
		{
			get => _fieldlimit;
			set
			{
				if (!Locked) _fieldlimit = value;
				else throw new TemplateAccessException();
			}
		}

		private int _timelimit;
		public int TimeLimit
		{
			get => _timelimit;
			set
			{
				if (!Locked) _timelimit = value;
				else throw new TemplateAccessException();
			}
		}

		private MapPortalTemplate[] _portals;
		public MapPortalTemplate[] Portals
		{
			get => _portals;
			set
			{
				if (!Locked) _portals = value;
				else throw new TemplateAccessException();
			}
		}

		private MapReactorTemplate[] _reactors;
		public MapReactorTemplate[] Reactors
		{
			get => _reactors;
			set
			{
				if (!Locked) _reactors = value;
				else throw new TemplateAccessException();
			}
		}

		private MapLifeTemplate[] _life;
		public MapLifeTemplate[] Life
		{
			get => _life;
			set
			{
				if (!Locked) _life = value;
				else throw new TemplateAccessException();
			}
		}

		private MapFootholdTemplate[] _footholds;
		public MapFootholdTemplate[] Footholds
		{
			get => _footholds;
			set
			{
				if (!Locked) _footholds = value;
				else throw new TemplateAccessException();
			}
		}

		public MapTemplate(int nMapId)
			: base(nMapId)
		{
			_portals = new MapPortalTemplate[0];
			_reactors = new MapReactorTemplate[0];
			_life = new MapLifeTemplate[0];
			_footholds = new MapFootholdTemplate[0];
		}

		public override void LockTemplate()
		{
			foreach (var p in Portals) p.LockTemplate();
			foreach (var l in Life) l.LockTemplate();
			foreach (var f in Footholds) f.LockTemplate();
			foreach (var r in Reactors) r.LockTemplate();

			base.LockTemplate();
		}

		public MapPortalTemplate PortalByPortalName(string sPortalName)
			=> Portals.FirstOrDefault(p => p.sPortalName == sPortalName);
		public MapLifeTemplate LifeByTemplate(int templateId)
			=> Life.FirstOrDefault(l => l.TemplateId == templateId);
		public MapFootholdTemplate FootholdByTemplate(int fhID)
			=> Footholds.FirstOrDefault(fh => fh.TemplateId == fhID);
		public MapReactorTemplate ReactorBySpawnIndex(int spawnIndex)
			=> Reactors.FirstOrDefault(r => r.SpawnIndex == spawnIndex);

		public bool HasMoveLimit() => (FieldLimit & FieldOpt.FIELDOPT_MOVELIMIT) != 0;
		public bool HasSkillLimit() => (FieldLimit & FieldOpt.FIELDOPT_SKILLLIMIT) != 0;
		public bool HasSummonLimit() => (FieldLimit & FieldOpt.FIELDOPT_SUMMONLIMIT) != 0;
		public bool HasMysticDoorLimit() => (FieldLimit & FieldOpt.FIELDOPT_MYSTICDOORLIMIT) != 0;
		public bool HasMigrateLimit() => (FieldLimit & FieldOpt.FIELDOPT_MIGRATELIMIT) != 0;
		public bool HasPortalScrollLimit() => (FieldLimit & FieldOpt.FIELDOPT_PORTALSCROLLLIMIT) != 0;
		public bool HasTeleportItemLimit() => (FieldLimit & FieldOpt.FIELDOPT_TELEPORTITEMLIMIT) != 0;
		public bool HasNoExpDecrease() => (FieldLimit & FieldOpt.FIELDOPT_NOEXPDECREASE) != 0;
		public bool HasCashWeatherConsumeLimit() => (FieldLimit & FieldOpt.FIELDOPT_CASHWEATHERCONSUMELIMIT) != 0;
		public bool HasStatChangeItemLimit() => (FieldLimit & FieldOpt.FIELDOPT_STATCHANGEITEMCONSUMELIMIT) != 0;
		public bool HasNoPetLimit() => (FieldLimit & FieldOpt.FIELDOPT_NOPET) != 0;
		public bool HasDropLimit() => (FieldLimit & FieldOpt.FIELDOPT_DROPLIMIT) != 0;
		public bool HasPartyBossChangeLimit() => (FieldLimit & FieldOpt.FIELDOPT_PARTYBOSSCHANGELIMIT) != 0;
	}
}
