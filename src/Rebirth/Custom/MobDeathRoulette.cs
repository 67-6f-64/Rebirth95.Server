using Rebirth.Characters;
using Rebirth.Characters.Modify;
using Rebirth.Game;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.Item.Consume;
using static Rebirth.Game.FieldEffectTypes;

namespace Rebirth.Custom
{
    /// <summary>
    /// Putting it here for now and then we can decide where it goes later if we keep it
    /// </summary>
    public class MobDeathRoulette
    {
        // RR1: -- Scroll Equip Class
        // 0: Warrior
        // 1: Magician
        // 2: Bowperson
        // 3: Thief
        // 4: Pirate

        // RR2: Scroll Equip Type -- Body Part Index
        // 0: Helmet -- 0x1
        // 1: Topwear -- 0x5
        // 2: Gloves -- 0x8
        // 3: Boots -- 0x7
        // 4: Weapon -- 0xB

        // RR3: -- Scroll Success Chance
        // 0: 30
        // 1: 40
        // 2: 50
        // 3: 60
        // 4: 70
        // 5: 80 -- we arent using this one cuz we dont have 80% scrolls (yet ;))

        /// <summary>
        /// Creates the string location of the WZ effect to be sent to the client.
        /// </summary>
        /// <param name="nIdx">Roulette index (0-2), set to -1 to return the frame.</param>
        /// <returns>String WZ location of the given effect</returns>
        private string GetEffectString(int nIdx)
            => nIdx < 0
            ? "miro/frame"
            : $"miro/RR{nIdx + 1}/{Idxs[nIdx]}";

        /// <summary>
        /// Initialized and populated from the ItemTemplateManager
        /// </summary>
        public static readonly List<RouletteItem>[] Scrolls =
            {
                new List<RouletteItem>(), // 30
                new List<RouletteItem>(), // 40
                new List<RouletteItem>(), // 50
                new List<RouletteItem>(), // 60
                new List<RouletteItem>(), // 70
            };


        private readonly int[] Idxs = new int[3];
        private readonly int nScrollItemID;

        public MobDeathRoulette()
        {
            for (int i = 0; i < 3; i++)
            {
                Idxs[i] = Constants.Rand.Next(0, 5);

				if (Idxs[i] == 1)
					Idxs[i] = 3; // temp fix
            }

            var firstFlag = (RouletteItemFlags)(1 << Idxs[0]);
            var secondFlag = (RouletteItemFlags)(1 << (5 + Idxs[1]));

            nScrollItemID = Scrolls[Idxs[2]]
                .Where(i => i.IsMatch
                (firstFlag, secondFlag))
                .Random()
                .ItemID;
        }

        private static int GetIndexBySuccessRate(int nSuccessRate)
            => (nSuccessRate / 10) - 3;

        public static void TryInsertScroll(ConsumeItemTemplate template)
        {
            var sRate = template.SuccessRate;

            if (template.TradeBlock)
                return;

            if (template.SuccessRate % 10 != 0 || sRate > 70 || sRate < 30)
                return;

            var nListIdx = GetIndexBySuccessRate(sRate);

            var nFlag = RouletteItemFlags.None;

            var nIdx = ItemConstants.GetEquipTypeScrollID(template.TemplateId);
            switch (nIdx)
            {
                case EquipType.CAP:
                    nFlag |= RouletteItemFlags.Cap;
                    break;
                case EquipType.SHIRT:
                    nFlag |= RouletteItemFlags.Shirt;
                    break;
                case EquipType.SHOES:
                    nFlag |= RouletteItemFlags.Shoes;
                    break;
                case EquipType.GLOVES:
                    nFlag |= RouletteItemFlags.Gloves;
                    break;
                default:
                    {
                        switch ((WeaponType)nIdx)
                        {
                            case WeaponType.WT_OH_SWORD:
                            case WeaponType.WT_OH_MACE:
                            case WeaponType.WT_OH_AXE:
                            case WeaponType.WT_TH_AXE:
                            case WeaponType.WT_TH_MACE:
                            case WeaponType.WT_TH_SWORD:
                            case WeaponType.WT_SPEAR:
                            case WeaponType.WT_POLEARM: // poor warriors have so many weapon types :(
                                nFlag |= RouletteItemFlags.Warrior;
                                break;
                            case WeaponType.WT_DAGGER:
                            case WeaponType.WT_SUB_DAGGER:
                            case WeaponType.WT_THROWINGGLOVE:
                                nFlag |= RouletteItemFlags.Thief;
                                break;
                            case WeaponType.WT_WAND:
                            case WeaponType.WT_STAFF:
                                nFlag |= RouletteItemFlags.Magician;
                                break;
                            case WeaponType.WT_BOW:
                            case WeaponType.WT_CROSSBOW:
                                nFlag |= RouletteItemFlags.Archer;
                                break;
                            case WeaponType.WT_KNUCKLE:
                            case WeaponType.WT_GUN:
                                nFlag |= RouletteItemFlags.Pirate;
                                break;
                            default:
                                // not a weapon
                                return;
                        }

                        nFlag |= RouletteItemFlags.Weapon;

                        Scrolls[nListIdx]
                            .Add(new RouletteItem
                            {
                                ItemID = template.TemplateId,
                                ItemFlag = nFlag
                            });
                    }
                    return;
            }

            if (template.IncDEX > 0)
            {
                nFlag |= RouletteItemFlags.Archer;
                nFlag |= RouletteItemFlags.Pirate;
            }
            else if (template.IncSTR > 0)
            {
                nFlag |= RouletteItemFlags.Warrior;
            }
            else if (template.IncINT > 0)
            {
                nFlag |= RouletteItemFlags.Magician;
            }
            else if (template.IncLUK > 0)
            {
                nFlag |= RouletteItemFlags.Thief;
            }
            else if (template.IncPAD > 0)
            {
                nFlag |= RouletteItemFlags.AllJobs; // add all job flags
                nFlag = (nFlag & ~RouletteItemFlags.Magician); // turn off magician flag
            }
            else if (template.IncMAD > 0)
            {
                nFlag |= RouletteItemFlags.Magician;
            }
            else if (template.IncACC > 0)
            {
                nFlag |= RouletteItemFlags.AllJobs; // add all job flags
                nFlag = (nFlag & ~RouletteItemFlags.Magician); // turn off magician flag
            }
            else if (template.IncEVA > 0)
            {
                nFlag |= RouletteItemFlags.AllJobs; // add all job flags
                nFlag = (nFlag & ~RouletteItemFlags.Magician); // turn off magician flag
            }
            else if (template.IncMHP > 0)
            {
                nFlag |= RouletteItemFlags.AllJobs;
            }
            else if (template.IncMMP > 0)
            {
                nFlag |= RouletteItemFlags.AllJobs;
            }
            else if (template.IncJump > 0)
            {
                nFlag |= RouletteItemFlags.AllJobs;
            }
            else if (template.IncSpeed > 0)
            {
                nFlag |= RouletteItemFlags.AllJobs;
            }
            else if (template.IncPDD > 0)
            {
                nFlag |= RouletteItemFlags.AllJobs;
            }
            else if (template.IncMDD > 0)
            {
                nFlag |= RouletteItemFlags.AllJobs;
            }

            if (nFlag == RouletteItemFlags.None)
                return;

            Scrolls[nListIdx]
                .Add(new RouletteItem
                {
                    ItemID = template.TemplateId,
                    ItemFlag = nFlag
                });
        }

        public static void Attempt(Character c)
        {
            if (Constants.Rand.NextDouble() > Constants.MobDeathRoulette_Odds)
                return;

            var roulette = new MobDeathRoulette();

            for (int i = -1; i < 3; i++)
            {
                var effect = new FieldEffectPacket(FieldEffect.Screen)
                {
                    sName = roulette.GetEffectString(i)
                };

                effect.Broadcast(c, true);
            }

            var pItem = MasterManager.CreateItem(roulette.nScrollItemID);

            pItem.nNumber = 1;

            // todo delay this
            InventoryManipulator.InsertInto(c, pItem);
        }
    }

    [Flags]
    public enum RouletteItemFlags : short
    {
        None = 0,

        Warrior = 1,
        Magician = 2,
        Archer = 4,
        Thief = 8,
        Pirate = 16,

        AllJobs = Warrior | Magician | Archer | Thief | Pirate,

        Cap = 32,
        Shirt = 64,
        Gloves = 128,
        Shoes = 256,
        Weapon = 512
    }

    public struct RouletteItem
    {
        public int ItemID { get; set; }
        public RouletteItemFlags ItemFlag { get; set; }

        public bool IsMatch(RouletteItemFlags nJob, RouletteItemFlags nPart) 
            => (ItemFlag & nJob) > 0 && (ItemFlag & nPart) > 0;
    }
}
