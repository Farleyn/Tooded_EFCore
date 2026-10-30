using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Tooded_EFCore;

public partial class ArveForm : Form
{
    private readonly DataGridView tabelKorv = new();
    private readonly GroupBox gbArve = new();
    private readonly Label lblKokku = new();
    private readonly TextBox txtEmail = new();
    private readonly Button btnMiinus = new(), btnPluss = new(), btnEemalda = new(),
                            btnGenPDF = new(), btnAvaPDF = new(), btnSaadaEmail = new();

    private string? viimanePdf;

    public ArveForm()
    {
        InitializeComponent();
        LooLiides();
        NaitaKorvi();
    }

    private void LooLiides()
    {
        Text = "Ostukorv ja arve";
        ClientSize = new Size(800, 500);
        MinimumSize = new Size(780, 480);
        StartPosition = FormStartPosition.CenterScreen;

        tabelKorv.SetBounds(12, 12, 460, 400);
        tabelKorv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tabelKorv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabelKorv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabelKorv.BackgroundColor = SystemColors.Window;
        tabelKorv.RowHeadersVisible = false;
        tabelKorv.MultiSelect = false;
        tabelKorv.ReadOnly = true;
        tabelKorv.AllowUserToAddRows = false;
        tabelKorv.Columns.Add("Toode", "Toode");
        tabelKorv.Columns.Add("Kogus", "Kogus");
        tabelKorv.Columns.Add("Hind", "Hind (€)");
        tabelKorv.Columns.Add("Summa", "Summa (€)");
        tabelKorv.Columns["Toode"]!.FillWeight = 200;
        tabelKorv.Columns["Kogus"]!.FillWeight = 70;
        tabelKorv.Columns["Hind"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        tabelKorv.Columns["Summa"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        SeaNupp(btnMiinus, "-1", 12, 422, 60, 30, btnMiinus_Click);
        SeaNupp(btnPluss, "+1", 84, 422, 60, 30, btnPluss_Click);
        SeaNupp(btnEemalda, "Eemalda rida", 156, 422, 120, 30, btnEemalda_Click);

        lblKokku.AutoSize = true;
        lblKokku.Font = new Font(Font.FontFamily, 12f, FontStyle.Bold);
        lblKokku.Location = new Point(12, 466);

        foreach (var c in new Control[] { btnMiinus, btnPluss, btnEemalda, lblKokku })
            c.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

        gbArve.Text = "Arve";
        gbArve.SetBounds(484, 12, 304, 206);
        gbArve.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        txtEmail.SetBounds(12, 50, 280, 23);

        SeaNupp(btnGenPDF, "Genereeri PDF", 12, 88, 280, 30, btnGenPDF_Click);
        SeaNupp(btnAvaPDF, "Ava PDF", 12, 126, 280, 30, btnAvaPDF_Click);
        SeaNupp(btnSaadaEmail, "Saada e-postiga", 12, 164, 280, 30, btnSaadaEmail_Click);

        gbArve.Controls.AddRange(new Control[] {
            new Label { Text = "Saaja e-post:", Location = new Point(12, 28), AutoSize = true },
            txtEmail, btnGenPDF, btnAvaPDF, btnSaadaEmail });

        Controls.AddRange(new Control[] { tabelKorv, btnMiinus, btnPluss, btnEemalda, lblKokku, gbArve });
    }

    private static void SeaNupp(Button nupp, string tekst, int x, int y, int laius, int korgus, EventHandler kasitleja)
    {
        nupp.Text = tekst;
        nupp.SetBounds(x, y, laius, korgus);
        nupp.Click += kasitleja;
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

    private static void SoidaLaost()
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

            SoidaLaost();
            MessageBox.Show("Arve saadetud ja laoseis uuendatud!");
            Ostukorv.Tyhjenda();
            NaitaKorvi();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Viga: " + ex.Message);
        }
    }
}
