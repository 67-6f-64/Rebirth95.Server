namespace Rebirth.Tools.Formulas
{
    public class RandomItem
    {
        public static int RegularOre
            => new int[] {
                4010000, // Bronze Ore                    The ore of a light and weak bronze.
                4010001, // Steel Ore                     The ore of a tough steel
                4010002, // Mithril Ore                   The ore of a light, solid Mithril.
                4010003, // Adamantium Ore                The ore of a heavy, strong Adamantium
                4010004, // Silver Ore                    The ore of a shiny silver
                4010005, // Orihalcon Ore                 An ore of a very rare mineral, Orihalcon.
                4010006, // Gold Ore                      The ore of gold, a very rare mineral
                4010007, // Lidium Ore                    Lidium Ore that in formed under the sand.
                4020000, // Garnet Ore                    The ore of a red jewel.
                4020001, // Amethyst Ore                  The ore of a purple jewel.
                4020002, // AquaMarine Ore                The ore of a blue jewel.
                4020003, // Emerald Ore                   The ore of a green jewel.
                4020004, // Opal Ore                      The ore of a jewel with many colors.
                4020005, // Sapphire Ore                  The ore of a blue, transparent jewel.
                4020006, // Topaz Ore                     The ore of a yellow jewel
                4020007, // Diamond Ore                   The ore of a jewel that's transparent
                4020008, // Black Crystal Ore             An ore of a crystal that has dark powers stored in it
                4020009, // Piece of Time                 A raw ore of a gem that contains the power of time.
            }.Random();

        public static int CrystalOre
            => new int[] {
                4004000, // Power Crystal Ore             An ore of a crystal that possesses power.
                4004001, // Wisdom Crystal Ore            An ore of a crystal that possesses wisdom.
                4004002, // DEX Crystal Ore               An ore of a crystal that possesses dexterity.
                4004003, // LUK Crystal Ore               An ore of a crystal that possesses luck.
                4004004, // Dark Crystal Ore              An ore of a crystal that possesses dark power. An incredible power lay asleep in it.
            }.Random();

        public static int MagicPowder
            => new int[] {
                4007000, // Magic Powder (Brown)          A brown Magic Powder used to create armors.
                4007001, // Magic Powder (White)          A white Magic Powder used to create armors.
                4007002, // Magic Powder (Blue)           A blue Magic Powder used to create armors.
                4007003, // Magic Powder (Green)          A green Magic Powder used to create armors.
                4007004, // Magic Powder (Yellow)         A yellow Magic Powder used to create armors.
                4007005, // Magic Powder (Purple)         A purple Magic Powder used to create armors.
                4007006, // Magic Powder (Red)            A red Magic Powder used to create armors.
                4007007, // Magic Powder (Black)          A black Magic Powder used to create armors.
            }.Random();

        public static int OmokPiece
            => new int[] {
                4030010, // Omok Piece : Octopus          An octopus-shaped Omok piece to play Omok.
                4030011, // Omok Piece : Pig              A pig-shaped Omok piece to play Omok.
                4030013, // Omok Piece : Bloctopus        A Bloctopus-shaped Omok piece to play Omok.
                4030014, // Omok Piece : Pink Teddy       A Pink Teddy-shaped Omok piece to play Omok.
                4030015, // Omok Piece : Panda Teddy      A Panda Teddy-shaped Omok piece to play Omok.
                4030016, // Omok Piece : Trixter          A Trixter-shaped Omok piece to play Omok.
            }.Random();
    }
}
