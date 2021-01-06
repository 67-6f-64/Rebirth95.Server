namespace Rebirth.Tools.Formulas
{
    public static class NextLevel
    {
        private static readonly int[] n = Generate();

        public static int get_next_level_exp(int nLevel)
        {
           var v1 = nLevel;

            if (nLevel > 200)
                return 0x7FFFFFFF; // int.MaxValue -- 2147483647

			if (nLevel < 1)
                v1 = 1;

            return n[v1];
        }

        private static int[] Generate()
        {
            var n = new int[201];

            n[2] = 34;
            n[3] = 57;
            n[4] = 92;
            n[5] = 135;
            var v1 = 15;
            n[1] = 15;
            n[6] = 372;
            n[7] = 560;
            n[8] = 840;
            n[9] = 1242;
            var v2 = n[9];
            n[10] = v2;
            n[11] = v2;
            n[12] = v2;
            n[13] = v2;
            n[14] = v2;
            do
            {
                n[v1] = (int)(n[v1 - 1] * 1.2 + 0.5);
                ++v1;
            }
            while (v1 <= 29);
            var v3 = n[29];
            n[30] = v3;
            n[31] = v3;
            n[32] = v3;
            n[33] = v3;
            n[34] = v3;
            var v4 = 35;
            do
            {
                n[v4] = (int)(n[v4 - 1] * 1.2 + 0.5);
                ++v4;
            }
            while (v4 <= 39);
            var v5 = 40;
            do
            {
                n[v5] = (int)(n[v5 - 1] * 1.08 + 0.5);
                ++v5;
            }
            while (v5 <= 69);
            var v6 = n[69];
            n[70] = v6;
            n[71] = v6;
            n[72] = v6;
            n[73] = v6;
            n[74] = v6;
            var v7 = 75;
            do
            {
                n[v7] = (int)(n[v7 - 1] * 1.07 + 0.5);
                ++v7;
            }
            while (v7 <= 119);
            var v8 = n[119];
            n[120] = v8;
            n[121] = v8;
            n[122] = v8;
            n[123] = v8;
            n[124] = v8;
            var v9 = 125;
            do
            {
                n[v9] = (int)(n[v9 - 1] * 1.07 + 0.5);
                ++v9;
            }
            while (v9 <= 159);
            var v10 = 160;
            do
            {
                n[v10] = (int)(n[v10 - 1] * 1.06 + 0.5);
                ++v10;
            }
            while (v10 <= 199);
            n[200] = 0;

            return n;
        }
    }
}
