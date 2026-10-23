namespace Tooded_EFCore;

public static class Kaustad
{
    public static readonly string Juur = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ToodedApp");
    public static readonly string Pildid = Path.Combine(Juur, "Images");
    public static readonly string Arved = Path.Combine(Juur, "Arved");

    public static void Loo()
    {
        Directory.CreateDirectory(Pildid);
        Directory.CreateDirectory(Arved);
    }

    public static Image? LaePilt(string? failinimi)
    {
        if (string.IsNullOrEmpty(failinimi)) return null;
        string tee = Path.Combine(Pildid, failinimi);
        if (!File.Exists(tee)) return null;

        using var ajutine = Image.FromFile(tee);
        return new Bitmap(ajutine);
    }
}
