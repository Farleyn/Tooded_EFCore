namespace Tooded_EFCore;

public partial class FormMain : Form
{
    private readonly Button btnTooted = new(), btnPood = new(), btnArve = new(), btnKiosk = new(), btnValju = new();
    private readonly StatusStrip olek = new();
    private readonly ToolStripStatusLabel lblKasutaja = new();

    public FormMain()
    {
        InitializeComponent();
        Text = "Peamenüü";
        ClientSize = new Size(260, 282);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        SeaNupp(btnTooted, "Tootehaldus", 20, (s, e) => Ava(new Form2()));
        SeaNupp(btnPood, "Pood", 66, (s, e) => Ava(new Form3()));
        SeaNupp(btnArve, "Ostukorv ja arve", 112, (s, e) => Ava(new ArveForm()));
        SeaNupp(btnKiosk, "Kioskirežiim", 158, (s, e) => Ava(new KioskForm()));
        SeaNupp(btnValju, "Välju", 204, (s, e) => Close());

        lblKasutaja.Text = $"Kasutaja: {AktiivneKasutaja.Nimi} ({AktiivneKasutaja.Roll})";
        olek.Items.Add(lblKasutaja);

        Controls.AddRange(new Control[] { btnTooted, btnPood, btnArve, btnKiosk, btnValju, olek });

        btnTooted.Enabled = AktiivneKasutaja.OnAdmin;
        btnArve.Enabled = AktiivneKasutaja.VoibMuua;
        btnKiosk.Enabled = AktiivneKasutaja.VoibMuua;
        Text = $"Peamenüü – {AktiivneKasutaja.Nimi}";
    }

    private void SeaNupp(Button nupp, string tekst, int y, EventHandler kasitleja)
    {
        nupp.Text = tekst;
        nupp.SetBounds(30, y, 200, 36);
        nupp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        nupp.Click += kasitleja;
    }

    private static void Ava(Form vorm)
    {
        using (vorm)
            vorm.ShowDialog();
    }
}