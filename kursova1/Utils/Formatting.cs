using System;
using System.Numerics;

namespace SecureAuthSystem.Utils
{
    public static class Formatting
    {
        public static string ToShort(BigInteger x)
        {
            var s = x.ToString();
            return s.Length <= 20 ? s : s.Substring(0, 10) + "…" + s.Substring(s.Length - 10);
        }
    }
}
