using System.Security.Cryptography;

namespace CvTracker.Helpers;

public static class RefreshTokenHelper
{
    public static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
    
}