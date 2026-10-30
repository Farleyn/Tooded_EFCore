using System.Globalization;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class AdminForm : Form
{
    private readonly TabControl tabid = new();
    private readonly StatusStrip olek = new();
    private readonly ToolStripStatusLabel lblStatistika = new();

    private readonly DataGridView tabelKasutajad = new();
    private readonly TextBox txtKNimi = new(), txtKEmail = new(), txtKParool = new();
    private readonly ComboBox comboRoll = new();

    private readonly DataGridView tabelKategooriad = new();
    private readonly TextBox txtKatNimi = new(), txtKatKirjeldus = new();

    private readonly DataGridView tabelTooted = new();
    private readonly TextBox txtTKogus = new(), txtTHind = new();
    private readonly Label lblTToode = new();

    public AdminForm()
    {
        InitializeComponent();
        Text = "Admin paneel";
        ClientSize = new Size(900, 520);
        MinimumSize = new Size(860, 480);
        StartPosition = FormStartPosition.CenterScreen;

        tabid.Dock = DockStyle.Fill;
        tabid.TabPages.Add(LooKasutajadLeht());
        tabid.TabPages.Add(LooKategooriadLeht());
        tabid.TabPages.Add(LooToodedLeht());

        olek.Items.Add(lblStatistika);
        Controls.Add(tabid);
        Controls.Add(olek);
        tabid.BringToFront();

        LaeKoik();
    }

    private static void SeaTabel(DataGridView tabel)
    {
        tabel.Dock = DockStyle.Fill;
        tabel.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabel.BackgroundColor = SystemColors.Window;
        tabel.RowHeadersVisible = false;
        tabel.MultiSelect = false;
        tabel.ReadOnly = true;
        tabel.AllowUserToAddRows = false;
    }

    private static Label Silt(string tekst, int x, int y)
    {
        return new Label { Text = tekst, Location = new Point(x, y), AutoSize = true };
    }

    private static Button Nupp(string tekst, int x, int y, int laius, EventHandler kasitleja)
    {
        var nupp = new Button { Text = tekst };
        nupp.SetBounds(x, y, laius, 30);
        nupp.Click += kasitleja;
        return nupp;
    }

    private static int? ValitudId(DataGridView tabel)
    {
        if (tabel.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vali tabelist rida.");
            return null;
        }
        return (int)tabel.SelectedRows[0].Cells["Id"].Value;
    }

    private TabPage LooKasutajadLeht()
    {
        var leht = new TabPage("Kasutajad") { Padding = new Padding(12) };

        SeaTabel(tabelKasutajad);
        tabelKasutajad.CellClick += (s, e) => TaidaKasutaja(e.RowIndex);

        txtKNimi.SetBounds(92, 26, 220, 23);
        txtKEmail.SetBounds(92, 60, 220, 23);
        txtKParool.SetBounds(92, 94, 220, 23);
        txtKParool.UseSystemPasswordChar = true;
        comboRoll.SetBounds(92, 128, 220, 23);
        comboRoll.DropDownStyle = ComboBoxStyle.DropDownList;
        comboRoll.Items.AddRange(new object[] { "admin", "kassiir", "kasutaja" });
        comboRoll.SelectedIndex = 2;

        var gb = new GroupBox { Text = "Kasutaja andmed", Bounds = new Rectangle(12, 0, 324, 165) };
        gb.Controls.AddRange(new Control[] {
            Silt("Nimi:", 12, 30), Silt("E-post:", 12, 64),
            Silt("Parool:", 12, 98), Silt("Roll:", 12, 132),
            txtKNimi, txtKEmail, txtKParool, comboRoll });

        var vihje = new Label
        {
            Text = "Uuendamisel jäta parool tühjaks, kui seda ei muudeta.",
            Bounds = new Rectangle(12, 172, 324, 34)
        };

        var paneel = new Panel { Dock = DockStyle.Right, Width = 336 };
        paneel.Controls.AddRange(new Control[] {
            gb, vihje,
            Nupp("Lisa", 12, 214, 100, btnKLisa_Click),
            Nupp("Uuenda", 124, 214, 100, btnKUuenda_Click),
            Nupp("Kustuta", 236, 214, 100, btnKKustuta_Click),
            Nupp("Tühjenda väljad", 12, 254, 324, (s, e) => TyhjendaKasutaja()) });

        leht.Controls.Add(tabelKasutajad);
        leht.Controls.Add(paneel);
        tabelKasutajad.BringToFront();
        return leht;
    }

    private TabPage LooKategooriadLeht()
    {
        var leht = new TabPage("Kategooriad") { Padding = new Padding(12) };

        SeaTabel(tabelKategooriad);
        tabelKategooriad.CellClick += (s, e) => TaidaKategooria(e.RowIndex);

        txtKatNimi.SetBounds(92, 26, 220, 23);
        txtKatKirjeldus.SetBounds(92, 60, 220, 23);

        var gb = new GroupBox { Text = "Kategooria andmed", Bounds = new Rectangle(12, 0, 324, 100) };
        gb.Controls.AddRange(new Control[] {
            Silt("Nimetus:", 12, 30), Silt("Kirjeldus:", 12, 64),
            txtKatNimi, txtKatKirjeldus });

        var vihje = new Label
        {
            Text = "Kategooria kustutamisel kustutatakse ka selle tooted.",
            Bounds = new Rectangle(12, 108, 324, 34)
        };

        var paneel = new Panel { Dock = DockStyle.Right, Width = 336 };
        paneel.Controls.AddRange(new Control[] {
            gb, vihje,
            Nupp("Lisa", 12, 150, 100, btnKatLisa_Click),
            Nupp("Uuenda", 124, 150, 100, btnKatUuenda_Click),
            Nupp("Kustuta", 236, 150, 100, btnKatKustuta_Click),
            Nupp("Tühjenda väljad", 12, 190, 324, (s, e) => TyhjendaKategooria()) });

        leht.Controls.Add(tabelKategooriad);
        leht.Controls.Add(paneel);
        tabelKategooriad.BringToFront();
        return leht;
    }

    private TabPage LooToodedLeht()
    {
        var leht = new TabPage("Tooted") { Padding = new Padding(12) };

        SeaTabel(tabelTooted);
        tabelTooted.CellClick += (s, e) => TaidaToode(e.RowIndex);

        lblTToode.SetBounds(92, 26, 220, 20);
        lblTToode.AutoEllipsis = true;
        txtTKogus.SetBounds(92, 60, 220, 23);
        txtTHind.SetBounds(92, 94, 220, 23);

        var gb = new GroupBox { Text = "Toote andmed", Bounds = new Rectangle(12, 0, 324, 131) };
        gb.Controls.AddRange(new Control[] {
            Silt("Toode:", 12, 28), Silt("Kogus:", 12, 64), Silt("Hind (€):", 12, 98),
            lblTToode, txtTKogus, txtTHind });

        var vihje = new Label
        {
            Text = "Uute toodete lisamiseks ja piltide jaoks kasuta Tootehaldust.",
            Bounds = new Rectangle(12, 188, 324, 34)
        };

        var paneel = new Panel { Dock = DockStyle.Right, Width = 336 };
        paneel.Controls.AddRange(new Control[] {
            gb, vihje,
            Nupp("Salvesta", 12, 148, 156, btnTSalvesta_Click),
            Nupp("Kustuta", 180, 148, 156, btnTKustuta_Click) });

        leht.Controls.Add(tabelTooted);
        leht.Controls.Add(paneel);
        tabelTooted.BringToFront();
        return leht;
    }

    private void LaeKoik()
    {
        LaeKasutajad();
        LaeKategooriad();
        LaeTooted();
        UuendaStatistikat();
    }

    private void UuendaStatistikat()
    {
        using var db = new ToodedContext();
        lblStatistika.Text =
            $"Kasutajaid: {db.Kasutajad.Count()}   |   Kategooriaid: {db.Kategooriatabel.Count()}   |   " +
            $"Tooteid: {db.Toodetabel.Count()}   |   Laos kokku: {db.Toodetabel.Sum(t => t.Kogus)} tk";
    }

    private void LaeKasutajad()
    {
        using var db = new ToodedContext();
        tabelKasutajad.DataSource = db.Kasutajad
            .OrderBy(k => k.Nimi)
            .Select(k => new { k.Id, k.Nimi, k.Email, k.Roll })
            .ToList();

        tabelKasutajad.Columns["Id"]!.Visible = false;
        tabelKasutajad.Columns["Email"]!.HeaderText = "E-post";
    }

    private void TaidaKasutaja(int rida)
    {
        if (rida < 0) return;

        var r = tabelKasutajad.Rows[rida];
        txtKNimi.Text = r.Cells["Nimi"].Value?.ToString();
        txtKEmail.Text = r.Cells["Email"].Value?.ToString();
        comboRoll.SelectedItem = r.Cells["Roll"].Value?.ToString();
        txtKParool.Clear();
    }

    private void TyhjendaKasutaja()
    {
        txtKNimi.Clear();
        txtKEmail.Clear();
        txtKParool.Clear();
        comboRoll.SelectedIndex = 2;
    }

    private bool LoeKasutaja(Kasutaja k, bool uus, ToodedContext db)
    {
        string nimi = txtKNimi.Text.Trim();
        string email = txtKEmail.Text.Trim().ToLower();

        if (nimi.Length == 0)
        { MessageBox.Show("Sisesta nimi."); return false; }

        if (!MailAddress.TryCreate(email, out _))
        { MessageBox.Show("Sisesta korrektne e-posti aadress."); return false; }

        if (comboRoll.SelectedItem is not string roll)
        { MessageBox.Show("Vali roll."); return false; }

        if ((uus || txtKParool.Text.Length > 0) && txtKParool.Text.Length < 6)
        { MessageBox.Show("Parool peab olema vähemalt 6 märki."); return false; }

        if (db.Kasutajad.Any(x => x.Email == email && x.Id != k.Id))
        { MessageBox.Show("See e-post on juba kasutusel."); return false; }

        if (!uus && k.Roll == "admin" && roll != "admin")
        {
            if (k.Email == AktiivneKasutaja.Email)
            { MessageBox.Show("Oma rolli ei saa muuta."); return false; }

            if (!db.Kasutajad.Any(x => x.Roll == "admin" && x.Id != k.Id))
            { MessageBox.Show("Alles peab jääma vähemalt üks admin."); return false; }
        }

        k.Nimi = nimi;
        k.Email = email;
        k.Roll = roll;
        if (txtKParool.Text.Length > 0)
            k.ParoolHash = PasswordHelper.Hash(txtKParool.Text);
        return true;
    }

    private void btnKLisa_Click(object? sender, EventArgs e)
    {
        using var db = new ToodedContext();
        var uus = new Kasutaja();
        if (!LoeKasutaja(uus, true, db)) return;

        db.Kasutajad.Add(uus);
        db.SaveChanges();
        TyhjendaKasutaja();
        LaeKoik();
    }

    private void btnKUuenda_Click(object? sender, EventArgs e)
    {
        if (ValitudId(tabelKasutajad) is not int id) return;

        using var db = new ToodedContext();
        var k = db.Kasutajad.Find(id);
        if (k == null) return;

        bool omaKonto = k.Email == AktiivneKasutaja.Email;
        if (!LoeKasutaja(k, false, db)) return;

        db.SaveChanges();

        if (omaKonto)
        {
            AktiivneKasutaja.Nimi = k.Nimi;
            AktiivneKasutaja.Email = k.Email;
        }
        LaeKoik();
    }

    private void btnKKustuta_Click(object? sender, EventArgs e)
    {
        if (ValitudId(tabelKasutajad) is not int id) return;

        using var db = new ToodedContext();
        var k = db.Kasutajad.Find(id);
        if (k == null) return;

        if (k.Email == AktiivneKasutaja.Email)
        { MessageBox.Show("Oma kontot ei saa kustutada."); return; }

        if (k.Roll == "admin" && !db.Kasutajad.Any(x => x.Roll == "admin" && x.Id != k.Id))
        { MessageBox.Show("Alles peab jääma vähemalt üks admin."); return; }

        if (MessageBox.Show($"Kas kustutan kasutaja {k.Nimi}?", "Kinnitus",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        db.Kasutajad.Remove(k);
        db.SaveChanges();
        TyhjendaKasutaja();
        LaeKoik();
    }

    private void LaeKategooriad()
    {
        using var db = new ToodedContext();
        tabelKategooriad.DataSource = db.Kategooriatabel
            .OrderBy(k => k.Kategooria_nimetus)
            .Select(k => new
            {
                k.Id,
                Nimetus = k.Kategooria_nimetus,
                k.Kirjeldus,
                Tooteid = k.Tooded.Count
            })
            .ToList();

        tabelKategooriad.Columns["Id"]!.Visible = false;
    }

    private void TaidaKategooria(int rida)
    {
        if (rida < 0) return;

        var r = tabelKategooriad.Rows[rida];
        txtKatNimi.Text = r.Cells["Nimetus"].Value?.ToString();
        txtKatKirjeldus.Text = r.Cells["Kirjeldus"].Value?.ToString();
    }

    private void TyhjendaKategooria()
    {
        txtKatNimi.Clear();
        txtKatKirjeldus.Clear();
    }

    private bool LoeKategooria(Kategooria kat, ToodedContext db)
    {
        string nimi = txtKatNimi.Text.Trim();
        string kirjeldus = txtKatKirjeldus.Text.Trim();

        if (nimi.Length == 0)
        { MessageBox.Show("Sisesta kategooria nimetus."); return false; }

        if (db.Kategooriatabel.Any(x => x.Kategooria_nimetus == nimi && x.Id != kat.Id))
        { MessageBox.Show("Selline kategooria on juba olemas."); return false; }

        kat.Kategooria_nimetus = nimi;
        kat.Kirjeldus = kirjeldus.Length == 0 ? null : kirjeldus;
        return true;
    }

    private void btnKatLisa_Click(object? sender, EventArgs e)
    {
        using var db = new ToodedContext();
        var uus = new Kategooria();
        if (!LoeKategooria(uus, db)) return;

        db.Kategooriatabel.Add(uus);
        db.SaveChanges();
        TyhjendaKategooria();
        LaeKoik();
    }

    private void btnKatUuenda_Click(object? sender, EventArgs e)
    {
        if (ValitudId(tabelKategooriad) is not int id) return;

        using var db = new ToodedContext();
        var kat = db.Kategooriatabel.Find(id);
        if (kat == null || !LoeKategooria(kat, db)) return;

        db.SaveChanges();
        LaeKoik();
    }

    private void btnKatKustuta_Click(object? sender, EventArgs e)
    {
        if (ValitudId(tabelKategooriad) is not int id) return;

        using var db = new ToodedContext();
        var kat = db.Kategooriatabel.Find(id);
        if (kat == null) return;

        int tooteid = db.Toodetabel.Count(t => t.KategooriaId == id);
        string tekst = tooteid > 0
            ? $"Kategooriaga \"{kat.Kategooria_nimetus}\" kustutatakse ka {tooteid} toodet. Jätkan?"
            : $"Kas kustutan kategooria \"{kat.Kategooria_nimetus}\"?";

        if (MessageBox.Show(tekst, "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        db.Kategooriatabel.Remove(kat);
        db.SaveChanges();
        TyhjendaKategooria();
        LaeKoik();
    }

    private void LaeTooted()
    {
        using var db = new ToodedContext();
        tabelTooted.DataSource = db.Toodetabel
            .OrderBy(t => t.Kategooria.Kategooria_nimetus)
            .ThenBy(t => t.Toodenimetus)
            .Select(t => new
            {
                t.Id,
                Toode = t.Toodenimetus,
                Kategooria = t.Kategooria.Kategooria_nimetus,
                t.Kogus,
                t.Hind,
                t.Triipkood
            })
            .ToList();

        tabelTooted.Columns["Id"]!.Visible = false;
        tabelTooted.Columns["Hind"]!.HeaderText = "Hind (€)";
    }

    private void TaidaToode(int rida)
    {
        if (rida < 0) return;

        var r = tabelTooted.Rows[rida];
        lblTToode.Text = r.Cells["Toode"].Value?.ToString();
        txtTKogus.Text = r.Cells["Kogus"].Value?.ToString();
        txtTHind.Text = r.Cells["Hind"].Value?.ToString();
    }

    private void btnTSalvesta_Click(object? sender, EventArgs e)
    {
        if (ValitudId(tabelTooted) is not int id) return;

        if (!int.TryParse(txtTKogus.Text, out int kogus) || kogus < 0)
        { MessageBox.Show("Kogus peab olema täisarv (0 või rohkem)."); return; }

        if (!decimal.TryParse(txtTHind.Text.Replace(',', '.'), NumberStyles.Number,
                CultureInfo.InvariantCulture, out decimal hind) || hind < 0)
        { MessageBox.Show("Hind peab olema arv, nt 2,50."); return; }

        using var db = new ToodedContext();
        var toode = db.Toodetabel.Find(id);
        if (toode == null) return;

        toode.Kogus = kogus;
        toode.Hind = hind;
        db.SaveChanges();
        LaeKoik();
    }

    private void btnTKustuta_Click(object? sender, EventArgs e)
    {
        if (ValitudId(tabelTooted) is not int id) return;

        using var db = new ToodedContext();
        var toode = db.Toodetabel.Find(id);
        if (toode == null) return;

        if (MessageBox.Show($"Kas kustutan toote \"{toode.Toodenimetus}\"?", "Kinnitus",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        db.Toodetabel.Remove(toode);
        db.SaveChanges();
        lblTToode.Text = "";
        txtTKogus.Clear();
        txtTHind.Clear();
        LaeKoik();
    }
}
