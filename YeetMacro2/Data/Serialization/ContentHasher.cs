using System;
using System.Security.Cryptography;
using System.Text;

namespace YeetMacro2.Data.Serialization;

public class ContentHasher
{
    public static string Create(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }
    }
}
