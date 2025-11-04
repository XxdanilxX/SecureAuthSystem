using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SecureAuthSystem.Security
{
    public class ElGamal
    {
        private BigInteger p, g, x, y;
        private Random rnd = new Random();

        public void GenerateKeys()
        {
            p = 30803;
            g = 2;
            x = rnd.Next(1000, 5000);
            y = BigInteger.ModPow(g, x, p);
            File.WriteAllText(KeyFile, $"{p} {g} {x} {y}");
        }
        private void LoadKeys()
        {
            if (!File.Exists(KeyFile))
                throw new Exception("Ключі не знайдено. Спочатку зашифруйте текст.");

            var parts = File.ReadAllText(KeyFile).Split(' ');
            p = BigInteger.Parse(parts[0]);
            g = BigInteger.Parse(parts[1]);
            x = BigInteger.Parse(parts[2]);
            y = BigInteger.Parse(parts[3]);
        }


        public void EncryptTextToFile(string text, string outputFile)
        {
            GenerateKeys();
            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                foreach (byte b in System.Text.Encoding.UTF8.GetBytes(text))
                {
                    BigInteger k = rnd.Next(2, (int)p - 2);
                    BigInteger a = BigInteger.ModPow(g, k, p);
                    BigInteger bEnc = (BigInteger.Pow(y, (int)k) * b) % p;
                    sw.WriteLine($"{a} {bEnc}");
                }
            }
        }
        private const string KeyFile = "Data/keys.txt";
        
        public string DecryptFileToText(string inputFile)
        {
            LoadKeys();
            if (!File.Exists(inputFile))
                return "Файл не знайдено.";

            string[] lines = File.ReadAllLines(inputFile);
            List<byte> bytes = new List<byte>();

            foreach (string line in lines)
            {
                var parts = line.Split(' ');
                BigInteger a = BigInteger.Parse(parts[0]);
                BigInteger bEnc = BigInteger.Parse(parts[1]);
                BigInteger s = BigInteger.ModPow(a, x, p);
                BigInteger sInv = ModInverse(s, p);
                BigInteger m = (bEnc * sInv) % p;
                bytes.Add((byte)(int)m);
            }

            return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
        }

        private BigInteger ModInverse(BigInteger a, BigInteger mod)
        {
            BigInteger m0 = mod, t, q;
            BigInteger x0 = 0, x1 = 1;
            if (mod == 1) return 0;
            while (a > 1)
            {
                q = a / mod;
                t = mod;
                mod = a % mod; a = t;
                t = x0;
                x0 = x1 - q * x0;
                x1 = t;
            }
            if (x1 < 0) x1 += m0;
            return x1;
        }
    }
}
