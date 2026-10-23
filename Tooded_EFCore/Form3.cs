using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class Form3 : Form
{
    private readonly ToodedContext _db = new();
    private readonly TabControl tabPood = new();
    private readonly Panel paneelAlla = new();
    private readonly Label lblKorv = new();
    private readonly Button btnKorv = new();

    public Form3()
    {
        InitializeComponent();
        Text = "Pood";
        ClientSize = new Size(960, 620);
        MinimumSize = new Size(600, 400);
        StartPosition = FormStartPosition.CenterScreen;

        tabPood.Dock = DockStyle.Fill;

        paneelAlla.Dock = DockStyle.Bottom;
        paneelAlla.Height = 44;
        paneelAlla.Padding = new Padding(8);

        lblKorv.Dock = DockStyle.Fill;
        lblKorv.TextAlign = ContentAlignment.MiddleLeft;

        btnKorv.Text = "Ostukorv ja arve";
        btnKorv.Dock = DockStyle.Right;
        btnKorv.Width = 150;
        btnKorv.Click += btnKorv_Click;

        paneelAlla.Controls.Add(lblKorv);
        paneelAlla.Controls.Add(btnKorv);
        lblKorv.BringToFront();

        Controls.Add(tabPood);
        Controls.Add(paneelAlla);
        tabPood.BringToFront();

        LaePood();
        UuendaKorvi();
    }

    private void LaePood()
    {
        tabPood.TabPages.Clear();

        var kategooriad = _db.Kategooriatabel
            .Include(k => k.Tooded)
            .OrderBy(k => k.Kategooria_nimetus)
            .ToList();

        foreach (var kat in kategooriad)
        {
            var leht = new TabPage(kat.Kategooria_nimetus);
            var rida = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(6) };

            foreach (var toode in kat.Tooded.OrderBy(t => t.Toodenimetus))
                rida.Controls.Add(LooTootekaart(toode));

            leht.Controls.Add(rida);
            tabPood.TabPages.Add(leht);
        }
    }

    private Panel LooTootekaart(Toode t)
    {
        var kaart = new Panel { Size = new Size(170, 250), BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(6) };

        var pilt = new PictureBox
        {
            Bounds = new Rectangle(5, 5, 158, 130),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = Kaustad.LaePilt(t.Pilt)
        };

        var nimi = new Label
        {
            Bounds = new Rectangle(5, 140, 158, 20),
            Text = t.Toodenimetus,
            AutoEllipsis = true
        };

        var hind = new Label
        {
            Bounds = new Rectangle(5, 162, 158, 20),
            Text = $"{t.Hind:F2} €",
            Font = new Font(Font, FontStyle.Bold)
        };

        var laos = new Label
        {
            Bounds = new Rectangle(5, 184, 158, 18),
            Text = $"Laos: {t.Kogus} tk"
        };

        var kogus = new NumericUpDown
        {
            Bounds = new Rectangle(5, 214, 55, 23),
            Minimum = 1,
            Maximum = Math.Max(1, t.Kogus),
            Value = 1
        };

        var nupp = new Button
        {
            Bounds = new Rectangle(66, 212, 97, 27),
            Text = "Lisa korvi",
            Enabled = t.Kogus > 0
        };

        nupp.Click += (s, e) =>
        {
            int korvis = Ostukorv.Ridad.FirstOrDefault(r => r.Toode.Id == t.Id)?.Kogus ?? 0;
            if (korvis + (int)kogus.Value > t.Kogus)
            {
                MessageBox.Show($"Laos on ainult {t.Kogus} tk.");
                return;
            }
            Ostukorv.Lisa(t, (int)kogus.Value);
            UuendaKorvi();
        };

        pilt.Click += (s, e) =>
            MessageBox.Show($"{t.Toodenimetus}\nHind: {t.Hind:F2} €\nLaos: {t.Kogus} tk");

        kaart.Controls.AddRange(new Control[] { pilt, nimi, hind, laos, kogus, nupp });
        return kaart;
    }

    private void UuendaKorvi()
    {
        int tk = Ostukorv.Ridad.Sum(r => r.Kogus);
        lblKorv.Text = $"Ostukorvis: {tk} tk, kokku {Ostukorv.Kokku:F2} €";
    }

    private void btnKorv_Click(object? sender, EventArgs e)
    {
        using var vorm = new ArveForm();
        vorm.ShowDialog();
        UuendaKorvi();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _db.Dispose();
        base.OnFormClosed(e);
    }
}
