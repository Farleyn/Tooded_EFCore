namespace Tooded_EFCore;

public class TriipkoodForm : Form
{
    private readonly Bitmap pilt;
    private readonly string kood;

    public TriipkoodForm(string toode, string kood)
    {
        this.kood = kood;
        pilt = TriipkoodiJoonistaja.Joonista(kood);

        Text = string.IsNullOrWhiteSpace(toode) ? "Triipkood" : toode;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        int laius = Math.Max(pilt.Width + 24, 300);
        int nuppudeY = pilt.Height + 24;
        ClientSize = new Size(laius, nuppudeY + 42);

        var pictureKood = new PictureBox
        {
            Image = pilt,
            SizeMode = PictureBoxSizeMode.AutoSize,
            Location = new Point((laius - pilt.Width) / 2, 12)
        };

        var btnSalvesta = new Button { Text = "Salvesta PNG" };
        btnSalvesta.SetBounds(laius - 222, nuppudeY, 100, 30);
        btnSalvesta.Click += btnSalvesta_Click;

        var btnSulge = new Button { Text = "Sulge" };
        btnSulge.SetBounds(laius - 112, nuppudeY, 100, 30);
        btnSulge.Click += (s, e) => Close();

        CancelButton = btnSulge;
        Controls.AddRange(new Control[] { pictureKood, btnSalvesta, btnSulge });
    }

    private void btnSalvesta_Click(object? sender, EventArgs e)
    {
        using var dialoog = new SaveFileDialog
        {
            Filter = "PNG pilt|*.png",
            FileName = $"triipkood_{kood}.png"
        };
        if (dialoog.ShowDialog() != DialogResult.OK) return;

        pilt.Save(dialoog.FileName, System.Drawing.Imaging.ImageFormat.Png);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        pilt.Dispose();
        base.OnFormClosed(e);
    }
}
