namespace Tooded_EFCore;

public static class PasswordHelper
{
    public static string Hash(string parool) =>
        BCrypt.Net.BCrypt.HashPassword(parool);

    public static bool Verify(string parool, string rasi) =>
        BCrypt.Net.BCrypt.Verify(parool, rasi);
}
