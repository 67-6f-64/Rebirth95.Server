using System.Collections.Generic;

namespace Rebirth.Characters.Actions
{
    public class RandomBoxes
    {
        public static int GetRandom(int inputId)
        {
            switch (inputId)
            {
                case 2022336: // Secret Box
                    // hack this together for now
                    var retVal = new List<int>();
                    foreach (var x in new int[4])
                        retVal.AddRange(new int[] { 2001519, 2001517, 2002018, 2002017, 2001522, 2001521, 2002026 }); // potions
                    retVal.AddRange(new int[] { 2022459, 2022460, 2022461 }); // meso boost

                    return retVal.Random();
                case 2022564: // Aran Paper Box
                    //return new int[] { }.Random();
                case 2022662: // Evan's Paper Box
                    //return new int[] { }.Random();
                    break; // TODO add these items
                case 2022652: // Dragon Rider's Warrior Box
                    return new int[] { 1002379, 1040121, 1041123, 1060110, 1061122, 1072222, 1082141, 1032030, 1102231, 1092038, 1302056, 1312030, 1322045, 1402035, 1412021, 1422027, 1432030, 1442044, 1402073, 1432066, 1442090 }.Random();
                case 2022653: // Dragon Rider's Magician Box
                    return new int[] { 1050102, 1072226, 1002401, 1051101, 1082154, 1032030, 1102145, 1051192, 1372010, 1382058 }.Random();
                case 2022654: // Dragon Rider's Thief Box
                    return new int[] { 1002381, 1050097, 1051091, 1072214, 1082135, 1032030, 1102231, 1332052, 1472053, 1332051, 1332100, 1472069 }.Random();
                case 2022655: // Dragon Rider's Bowman Box
                    return new int[] { 1002406, 1072227, 1050108, 1051107, 1082158, 1032030, 1102231, 1452019, 1452020, 1452021, 1462015, 1462016, 1462017, 1452058, 1462076 }.Random(); // red set
                case 2022656: // Dragon Rider's Pirate Box
                    return new int[] { 1002646, 1052131, 1072318, 1082213, 1032030, 1482012, 1482051, 1492024 }.Random();
                case 2022570: // King Pepe Warrior Weapon Box
                    return new int[] { 1442082, 1432057, 1422045, 1412042, 1402064, 1322073, 1312045, 1302119 }.Random();
                case 2022571: // King Pepe Magician Weapon Box
                case 2022572: // King Pepe Bowman Weapon Box
                case 2022573: // King Pepe Thief Weapon Box
                case 2022574: // King Pepe Pirate Weapon Box
                    break;
                case 2022575: // King Pepe Warrior Armor Box
                    return new int[] { 1072399, 1061156, 1060134, 1041148, 1040145, 1002990 }.Random();
                case 2022576: // King Pepe Magician Armor Box
                case 2022577: // King Pepe Bowman Armor Box
                case 2022578: // King Pepe Thief Armor Box	
                case 2022579: // King Pepe Pirate Armor Box
                case 2022580: // King Pepe Warrior Box
                    break;
                case 2022581: // King Pepe Magician Box
                    return new int[] { 1072400, 1051191, 1050155, 1002991, 1382070, 1372053 }.Random();

                case 2022582: // King Pepe Bowman Box
                    return new int[] { 1060135, 1061157, 1041149, 1040146, 1002992, 1452073, 1462066 }.Random();

                case 2022583: // King Pepe Thief Box
                    return new int[] { 1002993, 1040147, 1041150, 1060136, 1061158, 1072402, 1472089, 1332088, 1342018 }.Random();

                case 2022584: // King Pepe Pirate Box
                    return new int[] { 1002994, 1052208, 1072403, 1482037, 1492038 }.Random();
            }
            return -1;
        }
    }
}
