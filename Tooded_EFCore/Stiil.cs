namespace Tooded_EFCore;

public static class Stiil
{
    public static readonly Color Taust = Color.FromArgb(243, 244, 246);
    public static readonly Color Peamine = Color.FromArgb(46, 107, 80);
    public static readonly Color PeamineTume = Color.FromArgb(37, 88, 65);
    public static readonly Color Punane = Color.FromArgb(178, 58, 48);
    public static readonly Color Joon = Color.FromArgb(214, 218, 223);
    public static readonly Color Tekst = Color.FromArgb(33, 37, 41);
    public static readonly Color Hall = Color.FromArgb(108, 117, 125);

    public static readonly Font TekstFont = new("Segoe UI", 10f);
    public static readonly Font PaksFont = new("Segoe UI", 10f, FontStyle.Bold);
    public static readonly Font VaikeFont = new("Segoe UI", 9f);
    public static readonly Font HindFont = new("Segoe UI Semibold", 12f);
    public static readonly Font KokkuFont = new("Segoe UI Semibold", 15f);
    public static readonly Font SuurFont = new("Segoe UI Semibold", 18f);

    public static void Vorm(Form vorm)
    {
        vorm.AutoScaleMode = AutoScaleMode.None;
        vorm.Font = TekstFont;
        vorm.BackColor = Taust;
        vorm.ForeColor = Tekst;
        vorm.StartPosition = FormStartPosition.CenterScreen;
    }

    public static Label Silt(string tekst, int x, int y, int laius)
    {
        return new Label
        {
            Text = tekst,
            Bounds = new Rectangle(x, y, laius, 20),
            Font = VaikeFont,
            ForeColor = Hall
        };
    }

    public static void Sisend(TextBox kast, int x, int y, int laius)
    {
        kast.SetBounds(x, y, laius, 28);
        kast.BorderStyle = BorderStyle.FixedSingle;
        kast.BackColor = Color.White;
    }

    public static void Raam(Panel paneel)
    {
        paneel.BackColor = Color.White;
        paneel.Paint += (s, e) =>
        {
            using var pliiats = new Pen(Joon);
            e.Graphics.DrawRectangle(pliiats, 0, 0, paneel.Width - 1, paneel.Height - 1);
        };
        paneel.Resize += (s, e) => paneel.Invalidate();
    }

    public static void Nupp(Button nupp, string tekst, int x, int y, int laius, int korgus,
                            EventHandler klikk, string tuup = "tavaline")
    {
        nupp.Text = tekst;
        nupp.SetBounds(x, y, laius, korgus);
        nupp.FlatStyle = FlatStyle.Flat;
        nupp.UseVisualStyleBackColor = false;
        nupp.Cursor = Cursors.Hand;
        nupp.Click += klikk;

        Color taust = Color.White;
        Color kiri = Tekst;
        Color raam = Joon;
        Color hover = Color.FromArgb(234, 237, 240);

        if (tuup == "esmane")
        {
            taust = Peamine;
            kiri = Color.White;
            raam = Peamine;
            hover = PeamineTume;
        }
        else if (tuup == "oht")
        {
            kiri = Punane;
            raam = Punane;
            hover = Color.FromArgb(250, 236, 234);
        }

        nupp.BackColor = taust;
        nupp.ForeColor = kiri;
        nupp.FlatAppearance.BorderSize = 1;
        nupp.FlatAppearance.BorderColor = raam;
        nupp.FlatAppearance.MouseOverBackColor = hover;
        nupp.FlatAppearance.MouseDownBackColor = hover;

        nupp.EnabledChanged += (s, e) =>
        {
            if (nupp.Enabled)
            {
                nupp.BackColor = taust;
                nupp.ForeColor = kiri;
                nupp.FlatAppearance.BorderColor = raam;
            }
            else
            {
                nupp.BackColor = Color.FromArgb(232, 234, 237);
                nupp.ForeColor = Color.FromArgb(150, 156, 162);
                nupp.FlatAppearance.BorderColor = Joon;
            }
        };
    }

    public static void Tabel(DataGridView tabel)
    {
        tabel.BackgroundColor = Color.White;
        tabel.BorderStyle = BorderStyle.None;
        tabel.RowHeadersVisible = false;
        tabel.ReadOnly = true;
        tabel.MultiSelect = false;
        tabel.AllowUserToAddRows = false;
        tabel.AllowUserToResizeRows = false;
        tabel.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        tabel.EnableHeadersVisualStyles = false;
        tabel.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        tabel.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        tabel.ColumnHeadersHeight = 36;
        tabel.ColumnHeadersDefaultCellStyle.BackColor = Peamine;
        tabel.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        tabel.ColumnHeadersDefaultCellStyle.SelectionBackColor = Peamine;
        tabel.ColumnHeadersDefaultCellStyle.Font = PaksFont;
        tabel.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);

        tabel.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        tabel.GridColor = Joon;
        tabel.RowTemplate.Height = 30;
        tabel.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
        tabel.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 232, 223);
        tabel.DefaultCellStyle.SelectionForeColor = Tekst;
        tabel.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
    }
}
