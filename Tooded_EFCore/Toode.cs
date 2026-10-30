using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

[Index(nameof(Triipkood), IsUnique = true)]
public class Toode
{
    public int Id { get; set; }
    public string Toodenimetus { get; set; } = "";
    public int Kogus { get; set; }

    [Precision(10, 2)]
    public decimal Hind { get; set; }

    public string? Pilt { get; set; }

    [MaxLength(32)]
    public string? Triipkood { get; set; }

    public int KategooriaId { get; set; }
    public Kategooria Kategooria { get; set; } = null!;
}
