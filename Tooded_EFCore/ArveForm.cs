using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Tooded_EFCore;

public partial class ArveForm : Form
{
    private readonly DataGridView tabelKorv = new();
    private readonly Panel paneel = new();
    private readonly Label lblKokku = new();
    private readonly TextBox txtEmail = new();
    private readonly Button btnMiinus = new(), btnPluss = new(), btnEemalda = new(),
                            btnGenPDF = new(), btnAvaPDF = new(), btnSaadaEmail = new();

    private string? viimanePdf;

    public ArveForm()
    {
        InitializeComponent();
        Stiil.Vorm(this);
        Text = "Ostukorv ja arve";
        ClientSize = new Size(780, 470);
        MinimumSize = new Size(700, 460);

        Stiil.Tabel(tabelKorv);
        tabelKorv.SetBounds(16, 16, 448, 340);
        tabelKorv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tabelKorv.Columns.Add("Toode", "Toode");
        tabelKorv.Columns.Add("Kogus", "Kogus");
        tabelKorv.Columns.Add("Hind", "Hind, €");
        tabelKorv.Columns.Add("Summa", "Summa, €");
        tabelKorv.Columns["Toode"]!.FillWeight = 220;
        tabelKorv.Columns["Kogus"]!.FillWeight = 70;
        tabelKorv.Columns["Hind"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        tabelKorv.Columns["Summa"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        Stiil.Nupp(btnMiinus, "-1", 16, 368, 56, 34, btnMiinus_Click);
        Stiil.Nupp(btnPluss, "+1", 80, 368, 56, 34, btnPluss_Click);
        Stiil.Nupp(btnEemalda, "Eemalda rida", 144, 368, 130, 34, btnEemalda_Click, "oht");
        foreach (var nupp in new[] { btnMiinus, btnPluss, btnEemalda })
            nupp.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

        lblKokku.SetBounds(16, 416, 448, 36);
        lblKokku.Font = Stiil.KokkuFont;
        lblKokku.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

        Stiil.Raam(paneel);
        paneel.SetBounds(480, 16, 284, 270);
        paneel.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        Stiil.Sisend(txtEmail, 16, 38, 252);

        Stiil.Nupp(btnGenPDF, "Genereeri PDF", 16, 86, 252, 38, btnGenPDF_Click);
        Stiil.Nupp(btnAvaPDF, "Ava PDF", 16, 132, 252, 34, btnAvaPDF_Click);
        Stiil.Nupp(btnSaadaEmail, "Saada e-postiga", 16, 184, 252, 40, btnSaadaEmail_Click, "esmane");

        var vihje = Stiil.Silt("Enne saatmist genereeri PDF.", 16, 236, 252);

        paneel.Controls.AddRange(new Control[] {
            Stiil.Silt("Saaja e-post", 16, 16, 252),
            txtEmail, btnGenPDF, btnAvaPDF, btnSaadaEmail, vihje });

        Controls.AddRange(new Control[] { tabelKorv, btnMiinus, btnPluss, btnEemalda, lblKokku, paneel });

        NaitaKorvi();
    }

    private void NaitaKorvi()
    {
        int valitud = tabelKorv.CurrentRow?.Index ?? -1;

        tabelKorv.Rows.Clear();
        foreach (var rida in Ostukorv.Ridad)
        {
            int i = tabelKorv.Rows.Add(rida.Toode.Toodenimetus, rida.Kogus,
                rida.Toode.Hind.ToString("F2"), rida.Summa.ToString("F2"));
            tabelKorv.Rows[i].Tag = rida;
        }

        if (valitud >= 0 && valitud < tabelKorv.Rows.Count)
            tabelKorv.CurrentCell = tabelKorv.Rows[valitud].Cells[0];

        lblKokku.Text = $"Kokku: {Ostukorv.Kokku:F2} €";
    }

    private OstukorviRida? ValitudRida()
    {
        if (tabelKorv.CurrentRow?.Tag is OstukorviRida rida) return rida;
        MessageBox.Show("Vali korvist rida.");
        return null;
    }

    private void btnPluss_Click(object? sender, EventArgs e)
    {
        if (ValitudRida() is not OstukorviRida rida) return;

        if (rida.Kogus >= rida.Toode.Kogus)
        {
            MessageBox.Show($"Laos on ainult {rida.Toode.Kogus} tk.");
            return;
        }
        rida.Kogus++;
        NaitaKorvi();
    }

    private void btnMiinus_Click(object? sender, EventArgs e)
    {
        if (ValitudRida() is not OstukorviRida rida) return;

        rida.Kogus--;
        if (rida.Kogus <= 0)
            Ostukorv.Ridad.Remove(rida);
        NaitaKorvi();
    }

    private void btnEemalda_Click(object? sender, EventArgs e)
    {
        if (ValitudRida() is not OstukorviRida rida) return;

        Ostukorv.Ridad.Remove(rida);
        NaitaKorvi();
    }

    private void btnGenPDF_Click(object? sender, EventArgs e)
    {
        if (Ostukorv.Ridad.Count == 0)
        {
            MessageBox.Show("Ostukorv on tühi.");
            return;
        }

        DateTime aeg = DateTime.Now;
        viimanePdf = Path.Combine(Kaustad.Arved, $"Arve_{aeg:yyyyMMdd_HHmmss}.pdf");

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

        dokument.Save(viimanePdf);
        MessageBox.Show($"PDF on loodud:\n{viimanePdf}");
    }

    private void btnAvaPDF_Click(object? sender, EventArgs e)
    {
        if (viimanePdf == null || !File.Exists(viimanePdf))
        {
            MessageBox.Show("Genereeri enne PDF.");
            return;
        }

        Process.Start(new ProcessStartInfo(viimanePdf) { UseShellExecute = true });
    }

    private void btnSaadaEmail_Click(object? sender, EventArgs e)
    {
        if (viimanePdf == null || !File.Exists(viimanePdf))
        {
            MessageBox.Show("Genereeri enne PDF.");
            return;
        }

        string saaja = txtEmail.Text.Trim();
        if (!MailAddress.TryCreate(saaja, out _))
        {
            MessageBox.Show("Sisesta korrektne e-posti aadress.");
            return;
        }

        const string saatja = "sinu.konto@gmail.com";
        const string rakenduseParool = "xxxx xxxx xxxx xxxx";

        try
        {
            using var kiri = new MailMessage(saatja, saaja, "Arve", "Arve on manuses.");
            kiri.Attachments.Add(new Attachment(viimanePdf));

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(saatja, rakenduseParool),
                EnableSsl = true
            };
            smtp.Send(kiri);

            MessageBox.Show("Arve saadetud!");
            Ostukorv.Tyhjenda();
            NaitaKorvi();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Viga: " + ex.Message);
        }
    }
}
