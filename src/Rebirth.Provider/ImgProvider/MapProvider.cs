using System;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.Map;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;

namespace Rebirth.Provider.ImgProvider
{
	public sealed class MapProvider : AbstractProvider<MapTemplate>
	{
		protected override string ProviderName => "Map";

		protected override bool LoadFromJSON => false; // doesnt work properly for some reason

		public MapProvider(WzFileSystem baseImgDir)
			: base(baseImgDir)
		{ }

		protected override void ProcessAdditionalData()
		{
			foreach (var pEntry in Values)
			{
				if (pEntry.FieldType == FieldType.BATTLEFIELD)
				{
					pEntry.FieldLimit |= FieldOpt.FIELDOPT_NOPET;
				}

				switch (pEntry.TemplateId)
				{
					case 910040002:
						pEntry.FieldType = FieldType.WAITINGROOM;
						pEntry.FieldLimit |= FieldOpt.FIELDOPT_PARTYBOSSCHANGELIMIT;
						break;
					case 104010200: // forest trail 2 - mano
					case 100020101: // mushmom forest trail - mushmom
					case 100020301: // blue mushmom forest - blue mushmom
					case 102020500: // gusty peak - stumpy
					case 103030300: // unseen danger - dyle
					case 103030400: // deep mire - dyle
					case 211041400: // forest of dead trees IV - riche
					case 211040500: // sharp cliff III
					case 211050000: // icy cold field - snow witch
					case 200010302: // garden of darkness II - eliza
					case 105020400: // cave exit - snack bar
					case 230020100: // seaweed tower - seruf
					case 221020701: // eos tower 4th floor - rombot
					case 220050200: // lost time 2 - timer
					case 240020402: // manons forest - manon
					case 240020102: // griffey forest - griffey
					case 240040401: // levtiathans canyon - leviathan
					case 250020300: // advanced practice field - master dummy
					case 250010304: // Territory of Wandering Bear - Tae Roon
					case 250010504: // Goblin Forest 2 - King Sage Cat
					case 251010101: // 60-Year-Old Herb Garden - Bamboo Warrior
					case 251010102: // 80-Year-Old Herb Garden - Giant Centipede
					case 260010500: // Dry Desert - Deo
					case 261010003: // Lab - Unit 103 - Rurumo
					case 261020300: // Lab - Area C-1 - Security Camera
					case 261030000: // Lab - Secret Basement Path - Chimera
					case 270010500: // Memory Lane 5 - Dodo
					case 270020500: // Road of Regrets 5 - Lilynouch
					case 270030500: // Road to Oblivion 5 - Lyka
					case 682000001: // Hollowed Ground - Headless Horseman
					case 610010100: // Twisted Paths - Bigfoot
					case 610010101: // Twisted Paths - Bigfoot
					case 610010102: // Twisted Paths - Bigfoot
					case 610010103: // Twisted Paths - Bigfoot
					case 610010104: // Twisted Paths - Bigfoot
						pEntry.FieldType = FieldType.CUSTOM_AREA_BOSS;
						break;
					case 910030000: // russian roulette map
						pEntry.FieldType = FieldType.CUSTOM_COMMUNITYEVENT_RUSSIANROULETTE;
						pEntry.FieldLimit = pEntry.FieldLimit
											| FieldOpt.FIELDOPT_MYSTICDOORLIMIT
											| FieldOpt.FIELDOPT_PORTALSCROLLLIMIT
											| FieldOpt.FIELDOPT_TELEPORTITEMLIMIT
											| FieldOpt.FIELDOPT_NOEXPDECREASE
											| FieldOpt.FIELDOPT_CASHWEATHERCONSUMELIMIT
											| FieldOpt.FIELDOPT_WEDDINGINVITATIONLIMIT
											| FieldOpt.FIELDOPT_MIGRATELIMIT
											| FieldOpt.FIELDOPT_SUMMONLIMIT
											| FieldOpt.FIELDOPT_DROPLIMIT
											| FieldOpt.FIELDOPT_NOPET
											| FieldOpt.FIELDOPT_PARTYBOSSCHANGELIMIT;
						break;
					case 180000001: // starter map
						pEntry.ForcedReturn =
							180000001; // otherwise chars are forced out of starter map before they select a job
						pEntry.FieldLimit = pEntry.FieldLimit
											| FieldOpt.FIELDOPT_MYSTICDOORLIMIT
											| FieldOpt.FIELDOPT_PORTALSCROLLLIMIT
											| FieldOpt.FIELDOPT_TELEPORTITEMLIMIT
											| FieldOpt.FIELDOPT_NOEXPDECREASE
											| FieldOpt.FIELDOPT_CASHWEATHERCONSUMELIMIT
											| FieldOpt.FIELDOPT_WEDDINGINVITATIONLIMIT
											| FieldOpt.FIELDOPT_MIGRATELIMIT
											| FieldOpt.FIELDOPT_PARTYBOSSCHANGELIMIT;
						break;
					case 270050100:
						pEntry.FieldType = FieldType.CUSTOM_PINKBEAN;
						pEntry.FieldLimit |= FieldOpt.FIELDOPT_PARTYBOSSCHANGELIMIT;
						break;
					case 100040100:
					case 100040200:
					case 100040300:
					case 100040400:
					case 100040500: // has FT=6 by default (dungeon map)
						pEntry.FieldType = FieldType.CUSTOM_DUNGEONRAID_GOLEMTEMPLE;

						pEntry.FieldLimit = pEntry.FieldLimit
											| FieldOpt.FIELDOPT_MYSTICDOORLIMIT
											| FieldOpt.FIELDOPT_PORTALSCROLLLIMIT
											| FieldOpt.FIELDOPT_TELEPORTITEMLIMIT
											| FieldOpt.FIELDOPT_NOEXPDECREASE
											| FieldOpt.FIELDOPT_CASHWEATHERCONSUMELIMIT
											| FieldOpt.FIELDOPT_WEDDINGINVITATIONLIMIT
											| FieldOpt.FIELDOPT_MIGRATELIMIT
											| FieldOpt.FIELDOPT_PARTYBOSSCHANGELIMIT;
						break;
					case 240060001:
					case 240060101:
					case 240060201:
						pEntry.FieldType = FieldType.HONTAIL;
						pEntry.FieldLimit |= FieldOpt.FIELDOPT_PARTYBOSSCHANGELIMIT;
						break;
				}
			}
		}

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			var mapDir = imgDir.GetChild(ProviderName) as NameSpaceDirectory;

			foreach (var mapCatDir in mapDir.SubDirectories)
			{
				foreach (var imgFile in mapCatDir.Files)
				{
					var nMapId = int.Parse(imgFile.Name.Substring(0, 9));
					var pEntry = new MapTemplate(nMapId);

					// get info node data
					if (imgFile.GetChild("info") is WzProperty infoNode)
					{
						pEntry.ForcedReturn = infoNode.GetInt32("forcedReturn");
						pEntry.ReturnMap = infoNode.GetInt32("returnMap");
						pEntry.FlyMap = infoNode.GetInt32("fly") > 0;
						pEntry.Town = infoNode.GetInt32("town") > 0;

						pEntry.OnFirstUserEnter = infoNode.GetString("onFirstUserEnter");
						pEntry.OnUserEnter = infoNode.GetString("onUserEnter");

						pEntry.DecHP = infoNode.GetInt32("decHP");
						pEntry.DecMP = infoNode.GetInt32("decMP");
						pEntry.DecInterval = infoNode.GetInt32("decInterval");
						pEntry.ProtectItem = infoNode.GetInt32("protectItem");
						pEntry.TimeLimit = infoNode.GetInt32("timeLimit");
						pEntry.FieldLimit = (FieldOpt)infoNode.GetInt32("fieldLimit");
						pEntry.FieldType = (FieldType)infoNode.GetInt32("fieldType");
					}

					// get foothold node data
					if (imgFile.GetChild("foothold") is WzProperty fhNode)
					{
						var fhs = new List<MapFootholdTemplate>();
						foreach (var fhNodeKvp in fhNode.GetAllChildren())
						{
							var page = fhNodeKvp.Value as WzProperty;
							foreach (var mass in page.GetAllChildren())
							{
								fhs.AddRange(((WzProperty)mass.Value).GetAllChildren()
									.Values
									.Cast<WzProperty>()
									.Select(fhImgData =>
										new MapFootholdTemplate(Convert.ToInt32(fhImgData.Name))
										{
											Next = fhImgData.GetInt8("next"),
											Prev = fhImgData.GetInt8("prev"),
											X1 = fhImgData.GetInt16("x1"),
											X2 = fhImgData.GetInt16("x2"),
											Y1 = fhImgData.GetInt16("y1"),
											Y2 = fhImgData.GetInt16("y2")
										}));
							}
						}

						pEntry.Footholds = fhs.ToArray();
					}

					// get portal node data
					if (imgFile.GetChild("portal") is WzProperty portalNodes)
					{
						pEntry.Portals = portalNodes.GetAllChildren()
							.Values
							.Cast<WzProperty>()
							.Select(portalData =>
								new MapPortalTemplate(Convert.ToInt32(portalData.GetName()))
								{
									nIndex = Convert.ToInt32(portalData.GetName()),
									sPortalName = portalData.GetString("pn"),
									sTargetName = portalData.GetString("tn"),
									Script = portalData.GetString("script"),
									nPortalType = portalData.GetInt32("pt"),
									nTargetMap = portalData.GetInt32("tm"),
									nX = portalData.GetInt16("x"),
									nY = portalData.GetInt16("y")
								})
							.ToArray();
					}

					// get life node data
					if (imgFile.GetChild("life") is WzProperty lifeNodes)
					{
						pEntry.Life = lifeNodes.GetAllChildren()
							.Values
							.Cast<WzProperty>()
							.Select(lifeData =>
								new MapLifeTemplate(lifeData.GetInt32("id"))
								{
									CY = lifeData.GetInt16("cy"),
									Hide = lifeData.GetInt32("hide") > 0,
									F = lifeData.GetInt32("f") > 0,
									Foothold = lifeData.GetInt16("fh"),
									MobTime = lifeData.GetInt32("mobTime"),
									Type = lifeData.GetString("type"),
									RX0 = lifeData.GetInt16("rx0"),
									RX1 = lifeData.GetInt16("rx1"),
									X = lifeData.GetInt16("x"),
									Y = lifeData.GetInt16("y"),
								})
							.ToArray();
					}

					// get reactor node data
					if (imgFile.GetChild("reactor") is WzProperty reactorNodes)
					{
						pEntry.Reactors = reactorNodes.GetAllChildren()
							.Values
							.Cast<WzProperty>()
							.Where(item => !item.Name.Equals("trashremove") && !item.Name.Equals("endevent"))
							.Select(reactorData =>
								new MapReactorTemplate(reactorData.GetInt32("id"))
								{
									SpawnIndex = Convert.ToInt32(reactorData.Name),
									Name = reactorData.GetString("name"),
									F = reactorData.GetInt32("f") > 0,
									ReactorTime = reactorData.GetInt32("reactorTime"),
									X = reactorData.GetInt16("x"),
									Y = reactorData.GetInt16("y")
								})
							.ToArray();
					}

					InsertItem(pEntry);

					imgFile.Unload();
				}
			}
		}
	}
}