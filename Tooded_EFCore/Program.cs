using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Kaustad.Loo();

        using (var db = new ToodedContext())
        {
            db.Database.Migrate();
            LooAlgkasutajad(db);
        }

        Application.Run(new LoginForm());
    }

    private static void LooAlgkasutajad(ToodedContext db)
    {
        var vajalikud = new[]
        {
            new Kasutaja { Nimi = "Admin", Email = "admin@admin.ee",
                           ParoolHash = PasswordHelper.Hash("admin123"), Roll = "admin" },
            new Kasutaja { Nimi = "Kassiir", Email = "kassiir@pood.ee",
                           ParoolHash = PasswordHelper.Hash("kassiir123"), Roll = "kassiir" },
            new Kasutaja { Nimi = "Kirill Fedulin", Email = "kirillfedulin@pood.ee",
                           ParoolHash = PasswordHelper.Hash("fedulin123"), Roll = "kasutaja" }
        };

        foreach (var k in vajalikud)
            if (!db.Kasutajad.Any(x => x.Email == k.Email))
                db.Kasutajad.Add(k);

        db.SaveChanges();
    }
}
