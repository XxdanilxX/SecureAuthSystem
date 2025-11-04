using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace SecureAuthSystem.Security
{
    public static class HashUtils
    {
        public static BigInteger HashBig(string text)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                return new BigInteger(bytes);
            }
        }
    }
}
