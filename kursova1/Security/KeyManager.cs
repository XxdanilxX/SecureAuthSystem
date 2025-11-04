using System;
using System.IO;
using System.Numerics;

namespace SecureAuthSystem.Security
{
    public class KeyManager
    {
        private readonly string file;
        public BigInteger P { get; private set; }
        public BigInteger G { get; private set; }
        public BigInteger X { get; private set; }
        public BigInteger Y { get; private set; }

        public KeyManager(string path)
        {
            file = path;
        }

        public bool TryLoad()
        {
            if (!File.Exists(file)) return false;
            var parts = File.ReadAllText(file).Split(' ');
            if (parts.Length != 4) return false;
            P = BigInteger.Parse(parts[0]);
            G = BigInteger.Parse(parts[1]);
            X = BigInteger.Parse(parts[2]);
            Y = BigInteger.Parse(parts[3]);
            return true;
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            File.WriteAllText(file, $"{P} {G} {X} {Y}");
        }

        public void Generate(int decimalDigits = 64)
        {
            // p = 2q+1, g — первісний корінь
            var p = PrimeUtils.GenerateSafePrimeWithDigits(decimalDigits);
            var q = (p - 1) / 2;

            BigInteger g;
            var rnd = new Random();
            while (true)
            {
                g = PrimeUtils.RandomBigIntBetween(2, p - 2);
                // g^( (p-1)/f ) != 1 for f∈{2,q}
                if (BigInteger.ModPow(g, 2, p) != 1 &&
                    BigInteger.ModPow(g, q, p) != 1)
                    break;
            }

            BigInteger x = PrimeUtils.RandomBigIntBetween(2, p - 2);
            BigInteger y = BigInteger.ModPow(g, x, p);

            P = p; G = g; X = x; Y = y;
            Save();
        }
    }
}
