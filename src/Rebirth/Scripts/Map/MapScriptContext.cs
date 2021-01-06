using Rebirth.Field;
using Rebirth.Game;
using System;
using Rebirth.Common.Types;
using static Rebirth.Game.FieldEffectTypes;

namespace Rebirth.Scripts.Map
{
    public class MapScriptContext : ScriptContextBase<MapScript>
    {
        //private CField Field => Script.Field;

        public MapScriptContext(MapScript script) : base(script) { }

        public bool IsFirstUserEnter => Script.IsFirstUserEnterScript;

        /// <summary>
        /// Assumes that the script is UserEnter, not FirstUserEnter
        /// </summary>
        public void SendEnterMapEffect()
        {
            string sName;

            if (Field.Template.FieldType == FieldType.DOJANG) // onUserEnter=dojang_Eff
            {
                // first difficulty: 925010100
                // second difficulty: 925020100
                // third difficulty: 925030100
                var nRound = (Field.MapId % 10000) / 100;

                Script.Parent.Character.SendPacket(
                    new FieldEffectPacket(FieldEffect.Screen)
                    { sName = "dojang/start/stage" }
                    .GenerateForBroadcast());

                sName = $"dojang/start/number/{nRound}";
            }
            else
            {
                switch (Field.MapId)
                {
                    case 106020000: // Mushroom Forest Field
                        sName = "temaD/enter/mushCatle"; // onUserEnter=TD_MC_title
                        break;
                    case 240070000: // Tera Forest Time Gate
                        sName = "temaD/enter/teraForest"; // onUserEnter=TD_NC_title
                        break;
                    case 240070100: // Average Town Entrance
                        sName = "temaD/enter/neoCity1"; // onUserEnter=TD_NC_title
                        break;
                    case 240070200: // Midnight Harbor Entrance
                        sName = "temaD/enter/neoCity2"; // onUserEnter=TD_NC_title
                        break;
                    case 240070300: // Bombed City Center Retail District
                        sName = "temaD/enter/neoCity3"; // onUserEnter=TD_NC_title
                        break;
                    case 240070400: // Ruined City Intersection
                        sName = "temaD/enter/neoCity4"; // onUserEnter=TD_NC_title
                        break;
                    case 240070500: // Dangerous Tower Lobby
                        sName = "temaD/enter/neoCity5"; // onUserEnter=TD_NC_title
                        break;
                    case 240070600: // Air Battleship Bow
                        sName = "temaD/enter/neoCity6"; // onUserEnter=TD_NC_title
                        break;
                    case 10000: // Mushroom Park
                                // onUserEnter=go10000
                    case 20000: // Snail Park
                                // onUserEnter=go20000
                    case 30000: // Snail Garden
                                // onUserEnter=go30000
                    case 40000: // Inside the Small Forest
                                // onUserEnter=go40000
                    case 50000: // Inside the Dangerous Forest
                                // onUserEnter=go50000
                    case 1000000: // Amherst
                                  // onUserEnter=go1000000
                    case 1010000: // Entrance to Adventurer Training Center
                                  // onUserEnter=go1010000
                    case 1010100: // Adventurer Training Center 1
                                  // onUserEnter=go1010100
                    case 1010200: // Adventurer Training Center 2
                                  // onUserEnter=go1010200
                    case 1010300: // Adventurer Training Center 3
                                  // onUserEnter=go1010300
                    case 1010400: // Adventurer Training Center 4
                                  // onUserEnter=go1010400
                    case 1020000: // Split Road of Destiny
                                  // onUserEnter=go1020000
                    case 2000000: // Southperry
                                  // onUserEnter=go2000000
                    case 104000000: // Rith Harbor
                                    // onUserEnter=explorationPoint
                        sName = $"maplemap/enter/{Field.MapId}";
                        break;
                    default:
                        throw new InvalidOperationException($"No enter map effect exists for map ID: {Field.MapId}");
                }
            }

            Script.Parent.Character.SendPacket(
                new FieldEffectPacket(FieldEffect.Screen)
                { sName = sName }
                .GenerateForBroadcast());
        }
    }
}
