using System.Security.Cryptography;
using System.Text;

namespace AuthStream.API.Helpers;

public static class PasswordHelper
{
    public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        var algorithm = new HMACSHA512();

        // Gets computed hash of password and generated salt
        passwordSalt = algorithm.Key;
        passwordHash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        var algorithm = new HMACSHA512(passwordSalt);

        // Tries reproduce user's passwordHash using salt and password
        var computedHash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));

        // Checks newly computed hash against stored hash
        return computedHash.SequenceEqual(passwordHash);
    }
}