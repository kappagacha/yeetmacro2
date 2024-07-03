using System.Security.Cryptography;
using System.Text;

namespace YeetMacro2.Data.Serialization;

public class ContentHasher
{
    public static string Create(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
    }
}
