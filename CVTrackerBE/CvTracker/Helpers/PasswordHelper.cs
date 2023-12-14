using System.Security.Cryptography;
using BCrypt.Net;
namespace CvTracker.Helpers;

public static class PasswordHelper
{
    public static string GenerateRandomSalt()
    {
        return BCrypt.Net.BCrypt.GenerateSalt();
    }
    public static string GenerateHashedPwd(string inputPassword, string saltedString)
    {
        return BCrypt.Net.BCrypt.HashPassword(inputPassword, saltedString);
    }

    public static bool VerifyPassword(string plainText, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(plainText, hash);
    }
    
    
}