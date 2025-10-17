using System.Text;
using System.Security.Cryptography;
namespace Chibest.Service.Utilities;

public static class StringUtils
{
    public static string HashStringSHA256(string input)//SHA-256 Algorithm (1 way)
    {
        using SHA256 sha256Hasher = SHA256.Create();
        // ComputeHash - returns byte array  
        byte[] bytes = sha256Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Convert byte array to a string   
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}