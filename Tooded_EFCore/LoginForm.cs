namespace Tooded_EFCore;

public partial class LoginForm : Form
{
    private readonly TextBox txtEmail = new(), txtParool = new();
    private readonly Button btnLogin = new(), btnSulge = new();
    private readonly Label lblVeateade = new();

    public LoginForm()
    {
        InitializeComponent();
        Text = "Sisselogimine";
        ClientSize = new Size(320, 180);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var lblEmail = new Label { Text = "E-post:", Location = new Point(20, 25), AutoSize = true };
        var lblParool = new Label { Text = "Parool:", Location = new Point(20, 65), AutoSize = true };

        txtEmail.SetBounds(90, 22, 210, 23);
        txtParool.SetBounds(90, 62, 210, 23);
        txtParool.UseSystemPasswordChar = true;

        btnLogin.Text = "Logi sisse";
        btnLogin.SetBounds(90, 102, 100, 30);
        btnLogin.Click += btnLogin_Click;

        btnSulge.Text = "Sulge";
        btnSulge.SetBounds(200, 102, 100, 30);
        btnSulge.Click += (s, e) => Close();

        lblVeateade.SetBounds(20, 145, 280, 20);
        lblVeateade.ForeColor = Color.Red;

        AcceptButton = btnLogin;
        CancelButton = btnSulge;

        Controls.AddRange(new Control[] { lblEmail, lblParool, txtEmail, txtParool,
            btnLogin, btnSulge, lblVeateade });
    }

    private void btnLogin_Click(object? sender, EventArgs e)
    {
        string email = txtEmail.Text.Trim().ToLower();

        using var db = new ToodedContext();
        var kasutaja = db.Kasutajad.FirstOrDefault(k => k.Email == email);

        if (kasutaja == null || !PasswordHelper.Verify(txtParool.Text, kasutaja.ParoolHash))
        {
            lblVeateade.Text = "Vale e-post või parool.";
            txtParool.Clear();
            return;
        }

        AktiivneKasutaja.Nimi = kasutaja.Nimi;
        AktiivneKasutaja.Email = kasutaja.Email;
        AktiivneKasutaja.Roll = kasutaja.Roll;

        if (AktiivneKasutaja.OnAdmin)
        {
            var adminPaneel = new AdminForm();
            adminPaneel.FormClosed += (s, a) => Close();
            adminPaneel.Show();
        }
        else
        {
            var menyy = new FormMain();
            menyy.FormClosed += (s, a) => Close();
            menyy.Show();
        }
        Hide();
    }
}