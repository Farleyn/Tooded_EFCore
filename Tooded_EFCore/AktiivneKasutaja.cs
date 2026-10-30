namespace Tooded_EFCore;

public static class AktiivneKasutaja
{
    public static string Nimi { get; set; } = "";
    public static string Roll { get; set; } = "";
    public static bool OnAdmin => Roll == "admin";
    public static bool OnKassiir => Roll == "kassiir";
    public static bool VoibMuua => OnAdmin || OnKassiir;
}
