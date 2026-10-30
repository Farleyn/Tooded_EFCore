namespace Tooded_EFCore;

public static class TriipkoodiJoonistaja
{
    private static readonly string[] L = { "0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011" };
    private static readonly string[] G = { "0100111", "0110011", "0011011", "0100001", "0011101", "0111001", "0000101", "0010001", "0001001", "0010111" };
    private static readonly string[] R = { "1110010", "1100110", "1101100", "1000010", "1011100", "1001110", "1010000", "1000100", "1001000", "1110100" };
    private static readonly string[] Paarsus = { "LLLLLL", "LLGLGG", "LLGGLG", "LLGGGL", "LGLLGG", "LGGLLG", "LGGGLL", "LGLGLG", "LGLGGL", "LGGLGL" };

    private static readonly string[] Code128Mustrid =
    {
            "212222",
            "222122",
            "222221",
            "121223",
            "121322",
            "131222",
            "122213",
            "122312",
            "132212",
            "221213",
            "221312",
            "231212",
            "112232",
            "122132",
            "122231",
            "113222",
            "123122",
            "123221",
            "223211",
            "221132",
            "221231",
            "213212",
            "223112",
            "312131",
            "311222",
            "321122",
            "321221",
            "312212",
            "322112",
            "322211",
            "212123",
            "212321",
            "232121",
            "111323",
            "131123",
            "131321",
            "112313",
            "132113",
            "132311",
            "211313",
            "231113",
            "231311",
            "112133",
            "112331",
            "132131",
            "113123",
            "113321",
            "133121",
            "313121",
            "211331",
            "231131",
            "213113",
            "213311",
            "213131",
            "311123",
            "311321",
            "331121",
            "312113",
            "312311",
            "332111",
            "314111",
            "221411",
            "431111",
            "111224",
            "111422",
            "121124",
            "121421",
            "141122",
            "141221",
            "112214",
            "112412",
            "122114",
            "122411",
            "142112",
            "142211",
            "241211",
            "221114",
            "413111",
            "241112",
            "134111",
            "111242",
            "121142",
            "121241",
            "114212",
            "124112",
            "124211",
            "411212",
            "421112",
            "421211",
            "212141",
            "214121",
            "412121",
            "111143",
            "111341",
            "131141",
            "114113",
            "114311",
            "411113",
            "411311",
            "113141",
            "114131",
            "311141",
            "411131",
            "211412",
            "211214",
            "211232",
            "2331112"
    };

    public static bool OnEan13(string kood)
    {
        if (kood.Length != 13 || !kood.All(char.IsAsciiDigit)) return false;

        int summa = 0;
        for (int i = 0; i < 12; i++)
            summa += (kood[i] - '0') * (i % 2 == 0 ? 1 : 3);

        return (10 - summa % 10) % 10 == kood[12] - '0';
    }

    public static bool SobibJoonistamiseks(string kood)
    {
        return OnEan13(kood) || (kood.Length > 0 && kood.All(c => c >= 32 && c <= 126));
    }

    private static string Ean13(string kood)
    {
        string bitid = "101";
        for (int i = 0; i < 6; i++)
        {
            int number = kood[i + 1] - '0';
            bitid += Paarsus[kood[0] - '0'][i] == 'L' ? L[number] : G[number];
        }

        bitid += "01010";
        for (int i = 7; i < 13; i++)
            bitid += R[kood[i] - '0'];

        return bitid + "101";
    }

    private static string Code128(string kood)
    {
        var vaartused = new List<int> { 104 };
        int summa = 104;
        for (int i = 0; i < kood.Length; i++)
        {
            int vaartus = kood[i] - 32;
            vaartused.Add(vaartus);
            summa += vaartus * (i + 1);
        }

        vaartused.Add(summa % 103);
        vaartused.Add(106);

        string bitid = "";
        foreach (int vaartus in vaartused)
        {
            bool riba = true;
            foreach (char laius in Code128Mustrid[vaartus])
            {
                bitid += new string(riba ? '1' : '0', laius - '0');
                riba = !riba;
            }
        }
        return bitid;
    }

    public static Bitmap Joonista(string kood)
    {
        string bitid = OnEan13(kood) ? Ean13(kood) : Code128(kood);

        const int moodul = 3;
        const int vaikus = 12;
        const int korgus = 110;
        const int tekstiKorgus = 28;

        int laius = (bitid.Length + 2 * vaikus) * moodul;
        var pilt = new Bitmap(laius, korgus + tekstiKorgus);

        using var g = Graphics.FromImage(pilt);
        g.Clear(Color.White);

        for (int i = 0; i < bitid.Length; i++)
            if (bitid[i] == '1')
                g.FillRectangle(Brushes.Black, (vaikus + i) * moodul, 6, moodul, korgus - 6);

        using var font = new Font("Consolas", 12f);
        using var formaat = new StringFormat { Alignment = StringAlignment.Center };
        g.DrawString(kood, font, Brushes.Black, new RectangleF(0, korgus, laius, tekstiKorgus), formaat);

        return pilt;
    }
}
