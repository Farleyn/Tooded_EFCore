using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class Form2 : Form
{
    private readonly ToodedContext _db = new();
    private string? valitudPilt;

    private readonly DataGridView dataGridViewTooted = new();
    private readonly GroupBox gbAndmed = new(), gbPilt = new();
    private readonly TextBox txtNimetus = new(), txtKogus = new(), txtHind = new(), txtOtsing = new();
    private readonly ComboBox comboKategooria = new();
    private readonly PictureBox picturePilt = new();
    private readonly Button btnValiPilt = new(), btnLisa = new(), btnUuenda = new(),
                            btnKustuta = new(), btnTyhjenda = new();

    public Form2()
    {
        InitializeComponent();
        LooLiides();
        LaeKategooriad();
        LaeTooted();

        if (!AktiivneKasutaja.OnAdmin)
            btnLisa.Enabled = btnUuenda.Enabled = btnKustuta.Enabled = btnValiPilt.Enabled = false;
    }

    private void LooLiides()
    {
        Text = "Toodete haldus";
        ClientSize = new Size(960, 500);
        MinimumSize = new Size(900, 500);
        StartPosition = FormStartPosition.CenterScreen;

        txtOtsing.SetBounds(70, 13, 250, 23);
        txtOtsing.TextChanged += (s, e) => LaeTooted();

        dataGridViewTooted.SetBounds(12, 48, 600, 440);
        dataGridViewTooted.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dataGridViewTooted.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewTooted.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridViewTooted.BackgroundColor = SystemColors.Window;
        dataGridViewTooted.RowHeadersVisible = false;
        dataGridViewTooted.MultiSelect = false;
        dataGridViewTooted.ReadOnly = true;
        dataGridViewTooted.AllowUserToAddRows = false;
        dataGridViewTooted.CellClick += dataGridViewTooted_CellClick;

        gbAndmed.Text = "Toote andmed";
        gbAndmed.SetBounds(624, 12, 324, 165);
        gbAndmed.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        txtNimetus.SetBounds(92, 26, 220, 23);
        txtKogus.SetBounds(92, 60, 220, 23);
        txtHind.SetBounds(92, 94, 220, 23);
        comboKategooria.SetBounds(92, 128, 220, 23);
        comboKategooria.DropDownStyle = ComboBoxStyle.DropDownList;

        gbAndmed.Controls.AddRange(new Control[] {
            LooSilt("Nimetus:", 12, 30), LooSilt("Kogus:", 12, 64),
            LooSilt("Hind (€):", 12, 98), LooSilt("Kategooria:", 12, 132),
            txtNimetus, txtKogus, txtHind, comboKategooria });

        gbPilt.Text = "Pilt";
        gbPilt.SetBounds(624, 187, 324, 222);
        gbPilt.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        picturePilt.SetBounds(12, 22, 300, 150);
        picturePilt.SizeMode = PictureBoxSizeMode.Zoom;
        picturePilt.BorderStyle = BorderStyle.FixedSingle;

        SeaNupp(btnValiPilt, "Vali pilt...", 12, 184, 120, 28, btnValiPilt_Click);
        gbPilt.Controls.AddRange(new Control[] { picturePilt, btnValiPilt });

        SeaNupp(btnLisa, "Lisa", 624, 419, 100, 30, btnLisa_Click);
        SeaNupp(btnUuenda, "Uuenda", 736, 419, 100, 30, btnUuenda_Click);
        SeaNupp(btnKustuta, "Kustuta", 848, 419, 100, 30, btnKustuta_Click);
        SeaNupp(btnTyhjenda, "Tühjenda väljad", 624, 459, 324, 30, (s, e) => TyhjendaVorm());

        foreach (var nupp in new[] { btnLisa, btnUuenda, btnKustuta, btnTyhjenda })
            nupp.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        Controls.AddRange(new Control[] { LooSilt("Otsing:", 12, 17), txtOtsing, dataGridViewTooted, gbAndmed, gbPilt,
            btnLisa, btnUuenda, btnKustuta, btnTyhjenda });
    }

    private Label LooSilt(string tekst, int x, int y)
    {
        return new Label { Text = tekst, Location = new Point(x, y), AutoSize = true };
    }

    private void SeaNupp(Button nupp, string tekst, int x, int y, int laius, int korgus, EventHandler kasitleja)
    {
        nupp.Text = tekst;
        nupp.SetBounds(x, y, laius, korgus);
        nupp.Click += kasitleja;
    }

    private void LaeKategooriad()
    {
        comboKategooria.DataSource = _db.Kategooriatabel.OrderBy(k => k.Kategooria_nimetus).ToList();
        comboKategooria.DisplayMember = "Kategooria_nimetus";
        comboKategooria.ValueMember = "Id";
    }

    private void LaeTooted()
    {
        string otsing = txtOtsing.Text.Trim();

        dataGridViewTooted.DataSource = _db.Toodetabel
            .Where(t => t.Toodenimetus.Contains(otsing))
            .Select(t => new
            {
                t.Id,
                t.Toodenimetus,
                t.Kogus,
                t.Hind,
                t.KategooriaId,
                Kategooria = t.Kategooria.Kategooria_nimetus,
                t.Pilt
            })
            .ToList();

        dataGridViewTooted.Columns["Id"]!.Visible = false;
        dataGridViewTooted.Columns["KategooriaId"]!.Visible = false;
        dataGridViewTooted.Columns["Pilt"]!.Visible = false;
        dataGridViewTooted.Columns["Toodenimetus"]!.HeaderText = "Toode";
        dataGridViewTooted.Columns["Hind"]!.HeaderText = "Hind (€)";
    }

    private void btnValiPilt_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog { Filter = "Pildid|*.jpg;*.jpeg;*.png" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        valitudPilt = ofd.FileName;
        using var ajutine = Image.FromFile(valitudPilt);
        picturePilt.Image = new Bitmap(ajutine);
    }

    private bool LoeVorm(Toode toode)
    {
        if (string.IsNullOrWhiteSpace(txtNimetus.Text))
        { MessageBox.Show("Sisesta toote nimetus."); return false; }

        if (!int.TryParse(txtKogus.Text, out int kogus) || kogus < 0)
        { MessageBox.Show("Kogus peab olema täisarv (0 või rohkem)."); return false; }

        if (!decimal.TryParse(txtHind.Text.Replace(',', '.'), NumberStyles.Number,
                CultureInfo.InvariantCulture, out decimal hind)
            || hind < 0)
        { MessageBox.Show("Hind peab olema arv, nt 2,50."); return false; }

        if (comboKategooria.SelectedValue is not int kategooriaId)
        { MessageBox.Show("Vali kategooria."); return false; }

        toode.Toodenimetus = txtNimetus.Text.Trim();
        toode.Kogus = kogus;
        toode.Hind = hind;
        toode.KategooriaId = kategooriaId;

        if (valitudPilt != null)
        {
            toode.Pilt = Path.GetFileName(valitudPilt);
            string siht = Path.Combine(Kaustad.Pildid, toode.Pilt);
            if (!string.Equals(Path.GetFullPath(valitudPilt), siht, StringComparison.OrdinalIgnoreCase))
                File.Copy(valitudPilt, siht, true);
        }
        return true;
    }

    private int? ValitudId()
    {
        if (dataGridViewTooted.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vali tabelist toode.");
            return null;
        }
        return (int)dataGridViewTooted.SelectedRows[0].Cells["Id"].Value;
    }

    private void TyhjendaVorm()
    {
        txtNimetus.Clear();
        txtKogus.Clear();
        txtHind.Clear();
        valitudPilt = null;
        picturePilt.Image = null;
    }

    private void btnLisa_Click(object? sender, EventArgs e)
    {
        var uus = new Toode();
        if (!LoeVorm(uus)) return;

        _db.Toodetabel.Add(uus);
        _db.SaveChanges();
        TyhjendaVorm();
        LaeTooted();
    }

    private void dataGridViewTooted_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var rida = dataGridViewTooted.Rows[e.RowIndex];
        txtNimetus.Text = rida.Cells["Toodenimetus"].Value?.ToString();
        txtKogus.Text = rida.Cells["Kogus"].Value?.ToString();
        txtHind.Text = rida.Cells["Hind"].Value?.ToString();
        comboKategooria.SelectedValue = rida.Cells["KategooriaId"].Value;

        valitudPilt = null;
        picturePilt.Image = Kaustad.LaePilt(rida.Cells["Pilt"].Value?.ToString());
    }

    private void btnUuenda_Click(object? sender, EventArgs e)
    {
        if (ValitudId() is not int id) return;

        var toode = _db.Toodetabel.Find(id);
        if (toode == null || !LoeVorm(toode)) return;

        _db.SaveChanges();
        LaeTooted();
    }

    private void btnKustuta_Click(object? sender, EventArgs e)
    {
        if (ValitudId() is not int id) return;
        if (MessageBox.Show("Kas kustutan valitud toote?", "Kinnitus",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        var toode = _db.Toodetabel.Find(id);
        if (toode == null) return;

        _db.Toodetabel.Remove(toode);
        _db.SaveChanges();
        TyhjendaVorm();
        LaeTooted();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _db.Dispose();
        base.OnFormClosed(e);
    }
}
