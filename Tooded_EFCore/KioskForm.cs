using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class KioskForm : Form
{
    private readonly TabControl tabKategooriad = new();
    private readonly DataGridView tabelKorv = new();
    private readonly TextBox txtSkann = new();
    private readonly Label lblSonum = new(), lblKokku = new();
    private readonly Button btnMiinus = new(), btnPluss = new(), btnEemalda = new(),
                            btnTyhjenda = new(), btnMaksa = new(), btnValju = new();

    public KioskForm()
    {
        InitializeComponent();
        AutoScaleMode = AutoScaleMode.None;
        Text = "Kioskirežiim";
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        Font = new Font("Segoe UI", 12f);

        var paneelSkann = new Panel { Dock = DockStyle.Left, Width = 420 };
        txtSkann.SetBounds(120, 16, 280, 32);
        paneelSkann.Controls.Add(new Label { Text = "Triipkood:", Location = new Point(16, 20), AutoSize = true });
        paneelSkann.Controls.Add(txtSkann);
        txtSkann.KeyDown += txtSkann_KeyDown;

        btnValju.Text = "Välju";
        btnValju.Dock = DockStyle.Right;
        btnValju.Width = 140;
        btnValju.Click += (s, e) => Close();
        CancelButton = btnValju;

        lblSonum.Dock = DockStyle.Fill;
        lblSonum.TextAlign = ContentAlignment.MiddleLeft;

        var paneelUlal = new Panel { Dock = DockStyle.Top, Height = 64 };
        paneelUlal.Controls.Add(lblSonum);
        paneelUlal.Controls.Add(btnValju);
        paneelUlal.Controls.Add(paneelSkann);
        lblSonum.BringToFront();

        tabelKorv.Dock = DockStyle.Fill;
        tabelKorv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabelKorv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabelKorv.BackgroundColor = SystemColors.Window;
        tabelKorv.RowHeadersVisible = false;
        tabelKorv.MultiSelect = false;
        tabelKorv.ReadOnly = true;
        tabelKorv.AllowUserToAddRows = false;
        tabelKorv.RowTemplate.Height = 36;
        tabelKorv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        tabelKorv.ColumnHeadersHeight = 36;
        tabelKorv.Columns.Add("Toode", "Toode");
        tabelKorv.Columns.Add("Kogus", "Kogus");
        tabelKorv.Columns.Add("Summa", "Summa (€)");
        tabelKorv.Columns["Toode"]!.FillWeight = 200;
        tabelKorv.Columns["Kogus"]!.FillWeight = 70;
        tabelKorv.Columns["Summa"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        SeaNupp(btnMiinus, "-1", 0, 8, 134, 44, btnMiinus_Click);
        SeaNupp(btnPluss, "+1", 145, 8, 134, 44, btnPluss_Click);
        SeaNupp(btnEemalda, "Eemalda", 290, 8, 134, 44, btnEemalda_Click);

        lblKokku.SetBounds(0, 62, 424, 44);
        lblKokku.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
        lblKokku.TextAlign = ContentAlignment.MiddleLeft;

        SeaNupp(btnTyhjenda, "Tühjenda", 0, 118, 134, 60, btnTyhjenda_Click);
        SeaNupp(btnMaksa, "Maksa", 145, 118, 279, 60, btnMaksa_Click);
        btnMaksa.Font = new Font("Segoe UI", 16f, FontStyle.Bold);

        var paneelAlla = new Panel { Dock = DockStyle.Bottom, Height = 190 };
        paneelAlla.Controls.AddRange(new Control[] {
            btnMiinus, btnPluss, btnEemalda, lblKokku, btnTyhjenda, btnMaksa });

        var paneelParemal = new Panel { Dock = DockStyle.Right, Width = 440, Padding = new Padding(8) };
        paneelParemal.Controls.Add(tabelKorv);
        paneelParemal.Controls.Add(paneelAlla);
        tabelKorv.BringToFront();

        tabKategooriad.Dock = DockStyle.Fill;

        Controls.Add(tabKategooriad);
        Controls.Add(paneelParemal);
        Controls.Add(paneelUlal);
        tabKategooriad.BringToFront();

        LaeTooted();
        UuendaKorvi();

        ActiveControl = txtSkann;
        Shown += (s, e) => txtSkann.Focus();
    }

    private static void SeaNupp(Button nupp, string tekst, int x, int y, int laius, int korgus, EventHandler kasitleja)
    {
        nupp.Text = tekst;
        nupp.SetBounds(x, y, laius, korgus);
        nupp.Click += kasitleja;
    }

    private static Image? Pisipilt(string? failinimi)
    {
        using var pilt = Kaustad.LaePilt(failinimi);
        if (pilt == null) return null;

        double kordaja = Math.Min(64.0 / pilt.Width, 64.0 / pilt.Height);
        var suurus = new Size(Math.Max(1, (int)(pilt.Width * kordaja)), Math.Max(1, (int)(pilt.Height * kordaja)));
        return new Bitmap(pilt, suurus);
    }

    private void SeaSonum(string tekst, bool viga)
    {
        lblSonum.Text = tekst;
        lblSonum.ForeColor = viga ? Color.Red : SystemColors.ControlText;
    }

    private void LaeTooted()
    {
        string? valitud = tabKategooriad.SelectedTab?.Text;
        tabKategooriad.TabPages.Clear();

        using var db = new ToodedContext();
        var kategooriad = db.Kategooriatabel
            .AsNoTracking()
            .Include(k => k.Tooded)
            .OrderBy(k => k.Kategooria_nimetus)
            .ToList();

        foreach (var kat in kategooriad)
        {
            var leht = new TabPage(kat.Kategooria_nimetus);
            var rida = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(6) };

            foreach (var toode in kat.Tooded.OrderBy(t => t.Toodenimetus))
                rida.Controls.Add(LooNupp(toode));

            leht.Controls.Add(rida);
            tabKategooriad.TabPages.Add(leht);
        }

        var tagasi = tabKategooriad.TabPages.Cast<TabPage>().FirstOrDefault(p => p.Text == valitud);
        if (tagasi != null) tabKategooriad.SelectedTab = tagasi;
    }

    private Button LooNupp(Toode t)
    {
        var nupp = new Button
        {
            Size = new Size(170, 130),
            Margin = new Padding(6),
            Text = t.Kogus > 0 ? $"{t.Toodenimetus}\n{t.Hind:F2} €" : $"{t.Toodenimetus}\nOtsas",
            UseMnemonic = false,
            Image = Pisipilt(t.Pilt),
            ImageAlign = ContentAlignment.TopCenter,
            TextAlign = ContentAlignment.BottomCenter,
            TextImageRelation = TextImageRelation.ImageAboveText,
            Enabled = t.Kogus > 0
        };

        nupp.Click += (s, e) =>
        {
            LisaKorvi(t);
            txtSkann.Focus();
        };
        return nupp;
    }

    private void LisaKorvi(Toode t)
    {
        int korvis = Ostukorv.Ridad.FirstOrDefault(r => r.Toode.Id == t.Id)?.Kogus ?? 0;
        if (korvis + 1 > t.Kogus)
        {
            SeaSonum($"{t.Toodenimetus}: laos on ainult {t.Kogus} tk", true);
            return;
        }

        Ostukorv.Lisa(t, 1);
        SeaSonum($"Lisatud: {t.Toodenimetus}", false);
        UuendaKorvi();
    }

    private void UuendaKorvi()
    {
        int valitud = tabelKorv.CurrentRow?.Index ?? -1;

        tabelKorv.Rows.Clear();
        foreach (var rida in Ostukorv.Ridad)
        {
            int i = tabelKorv.Rows.Add(rida.Toode.Toodenimetus, rida.Kogus, rida.Summa.ToString("F2"));
            tabelKorv.Rows[i].Tag = rida;
        }

        if (valitud >= 0 && valitud < tabelKorv.Rows.Count)
            tabelKorv.CurrentCell = tabelKorv.Rows[valitud].Cells[0];

        lblKokku.Text = $"Kokku: {Ostukorv.Kokku:F2} €";
    }

    private OstukorviRida? ValitudRida()
    {
        if (tabelKorv.CurrentRow?.Tag is OstukorviRida rida) return rida;
        SeaSonum("Vali korvist rida.", true);
        return null;
    }

    private void txtSkann_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.SuppressKeyPress = true;

        string kood = txtSkann.Text.Trim();
        txtSkann.Clear();
        if (kood.Length == 0) return;

        using var db = new ToodedContext();
        var toode = db.Toodetabel.AsNoTracking().FirstOrDefault(t => t.Triipkood == kood);
        if (toode == null)
        {
            SeaSonum($"Triipkoodi ei leitud: {kood}", true);
            return;
        }

        LisaKorvi(toode);
    }

    private void btnPluss_Click(object? sender, EventArgs e)
    {
        if (ValitudRida() is not OstukorviRida rida) return;

        if (rida.Kogus >= rida.Toode.Kogus)
        {
            SeaSonum($"Laos on ainult {rida.Toode.Kogus} tk", true);
            return;
        }

        rida.Kogus++;
        UuendaKorvi();
        txtSkann.Focus();
    }

    private void btnMiinus_Click(object? sender, EventArgs e)
    {
        if (ValitudRida() is not OstukorviRida rida) return;

        rida.Kogus--;
        if (rida.Kogus <= 0)
            Ostukorv.Ridad.Remove(rida);
        UuendaKorvi();
        txtSkann.Focus();
    }

    private void btnEemalda_Click(object? sender, EventArgs e)
    {
        if (ValitudRida() is not OstukorviRida rida) return;

        Ostukorv.Ridad.Remove(rida);
        UuendaKorvi();
        txtSkann.Focus();
    }

    private void btnTyhjenda_Click(object? sender, EventArgs e)
    {
        if (Ostukorv.Ridad.Count == 0) return;

        if (MessageBox.Show("Kas tühjendan ostukorvi?", "Kinnitus",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        Ostukorv.Tyhjenda();
        UuendaKorvi();
        txtSkann.Focus();
    }

    private void btnMaksa_Click(object? sender, EventArgs e)
    {
        if (Ostukorv.Ridad.Count == 0)
        {
            SeaSonum("Ostukorv on tühi.", true);
            return;
        }

        try
        {
            decimal kokku = Ostukorv.Kokku;
            string pdf = ArveTeenus.LooPdf();
            ArveTeenus.SoidaLaost();
            Ostukorv.Tyhjenda();

            LaeTooted();
            UuendaKorvi();
            SeaSonum("Ost vormistatud.", false);
            MessageBox.Show($"Ost vormistatud.\nKokku: {kokku:F2} €\nArve: {pdf}");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Viga: " + ex.Message);
        }

        txtSkann.Focus();
    }
}
