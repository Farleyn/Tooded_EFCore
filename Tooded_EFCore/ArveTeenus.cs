using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Tooded_EFCore;

public static class ArveTeenus
{
    public static string LooPdf()
    {
        DateTime aeg = DateTime.Now;
        string tee = Path.Combine(Kaustad.Arved, $"Arve_{aeg:yyyyMMdd_HHmmss}.pdf");

        using var dokument = new PdfDocument();
        PdfPage leht = dokument.AddPage();
        leht.Width = XUnit.FromMillimeter(80);
        leht.Height = XUnit.FromPoint(210 + Ostukorv.Ridad.Count * 30);

        using (XGraphics gfx = XGraphics.FromPdfPage(leht))
        {
            var suur = new XFont("Arial", 16, XFontStyle.Bold);
            var tavaline = new XFont("Arial", 9);
            var paks = new XFont("Arial", 9, XFontStyle.Bold);
            var kokkuFont = new XFont("Arial", 14, XFontStyle.Bold);
            var joon = new XPen(XColors.Black, 0.5) { DashStyle = XDashStyle.Dash };

            double laius = leht.Width.Point;
            double vasak = 14;
            double parem = laius - 14;
            double y = 20;

            void Keskel(string tekst, XFont font) =>
                gfx.DrawString(tekst, font, XBrushes.Black,
                    new XRect(0, y, laius, 16), XStringFormats.TopCenter);

            void Vasakul(string tekst, XFont font) =>
                gfx.DrawString(tekst, font, XBrushes.Black,
                    new XRect(vasak, y, parem - vasak, 16), XStringFormats.TopLeft);

            void Paremal(string tekst, XFont font) =>
                gfx.DrawString(tekst, font, XBrushes.Black,
                    new XRect(vasak, y, parem - vasak, 16), XStringFormats.TopRight);

            void Joon() => gfx.DrawLine(joon, vasak, y + 3, parem, y + 3);

            Keskel("TOODETE POOD", suur);
            y += 24;
            Keskel($"Tšekk nr {aeg:yyMMddHHmm}", tavaline);
            y += 14;
            Joon();
            y += 10;

            Vasakul($"{aeg:dd.MM.yyyy}", tavaline);
            Paremal($"{aeg:HH:mm}", tavaline);
            y += 14;
            Vasakul($"Müüja: {AktiivneKasutaja.Nimi}", tavaline);
            y += 14;
            Joon();
            y += 10;

            foreach (var rida in Ostukorv.Ridad)
            {
                string nimi = rida.Toode.Toodenimetus;
                while (nimi.Length > 3 && gfx.MeasureString(nimi, paks).Width > parem - vasak)
                    nimi = nimi[..^1];

                Vasakul(nimi, paks);
                y += 13;
                Vasakul($"{rida.Kogus} tk × {rida.Toode.Hind:F2} €", tavaline);
                Paremal($"{rida.Summa:F2} €", tavaline);
                y += 17;
            }

            Joon();
            y += 10;
            Vasakul("KOKKU", kokkuFont);
            Paremal($"{Ostukorv.Kokku:F2} €", kokkuFont);
            y += 26;
            Vasakul($"Kaupu kokku: {Ostukorv.Ridad.Sum(r => r.Kogus)} tk", tavaline);
            y += 14;
            Joon();
            y += 10;

            Keskel("Aitäh ostu eest!", paks);
            y += 14;
            Keskel("Head päeva!", tavaline);
        }

        dokument.Save(tee);
        return tee;
    }

    public static void SoidaLaost()
    {
        using var db = new ToodedContext();
        foreach (var rida in Ostukorv.Ridad)
        {
            var toode = db.Toodetabel.Find(rida.Toode.Id);
            if (toode != null)
                toode.Kogus = Math.Max(0, toode.Kogus - rida.Kogus);
        }
        db.SaveChanges();
    }
}
