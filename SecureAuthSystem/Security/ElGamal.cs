using System;
using System.IO;
using System.Numerics;

namespace SecureAuthSystem.Security
{
    public class ElGamal
    {
        private const bool DebugMode = false;
        private readonly string keysPath = "Data/keys.txt";

        public BigInteger P { get; private set; }
        public BigInteger G { get; private set; }
        public BigInteger X { get; private set; }
        public BigInteger Y { get; private set; }

        public ElGamal()
        {
            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            if (!File.Exists(keysPath))
            {
                Console.WriteLine("🔑 Генеруємо нові ElGamal ключі...");
                GenerateKeys();
                SaveKeys();
            }
            else
            {
                LoadKeys();
            }
        }

        // 🔹 Шифрування файлу ------------------------------------------------------
        public void EncryptFile(string inputPath)
        {
            try
            {
                if (!File.Exists(inputPath))
                {
                    Console.WriteLine($"⚠ Файл {inputPath} не знайдено.");
                    return;
                }

                byte[] data = File.ReadAllBytes(inputPath);
                if (data.Length == 0)
                {
                    Console.WriteLine("⚠ Файл порожній, нічого не зашифровано.");
                    return;
                }

                using (StreamWriter sw = new StreamWriter("Data/close.txt"))
                {
                    foreach (byte b in data)
                    {
                        BigInteger k = PrimeUtils.RandomBigIntBetween(2, P - 2);
                        if (k <= 0) k = BigInteger.Abs(k) + 1;

                        BigInteger a = BigInteger.ModPow(G, k, P);
                        BigInteger bEnc = (BigInteger.ModPow(Y, k, P) * b) % P;

                        sw.WriteLine($"{a}:{bEnc}");
                    }
                }

                if (DebugMode)
                    Console.WriteLine("✅ Текст зашифровано і записано до close.txt");
            }
            catch (DivideByZeroException)
            {
                Console.WriteLine("❌ Помилка під час шифрування: поділ на нуль (p=0)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Помилка під час шифрування: {ex.Message}");
            }
        }

        // 🔹 Основна функція розшифрування ----------------------------------------
        public byte[] Decrypt(byte[] input)
        {
            try
            {
                string text = System.Text.Encoding.UTF8.GetString(input);
                var parts = text.Split(':');
                if (parts.Length != 2)
                    throw new FormatException("Невірний формат зашифрованих даних.");

                BigInteger a = BigInteger.Parse(parts[0]);
                BigInteger bEnc = BigInteger.Parse(parts[1]);
                BigInteger aX = BigInteger.ModPow(a, X, P);
                BigInteger aXInv = BigInteger.ModPow(aX, P - 2, P);
                BigInteger m = (bEnc * aXInv) % P;
                return m.ToByteArray();
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        // 🔹 Розшифровує файл у текст ---------------------------------------------
        public string DecryptFileToText(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                Console.WriteLine("⚠ Файл не знайдено.");
                return "";
            }

            var lines = File.ReadAllLines(inputFile);
            var output = new System.Text.StringBuilder();

            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length != 2) continue;

                BigInteger a = BigInteger.Parse(parts[0]);
                BigInteger bEnc = BigInteger.Parse(parts[1]);

                BigInteger aX = BigInteger.ModPow(a, X, P);
                BigInteger aXInv = BigInteger.ModPow(aX, P - 2, P);
                BigInteger m = (bEnc * aXInv) % P;

                output.Append((char)(int)m);
            }

            File.WriteAllText("Data/out.txt", output.ToString());
            return output.ToString();
        }

        // 🔹 Розшифровує файл у інший файл ---------------------------------------
        public void DecryptToFile(string inputFile, string outputFile)
        {
            string decryptedText = DecryptFileToText(inputFile);
            File.WriteAllText(outputFile, decryptedText);
        }

        // 🔹 Цифровий підпис ------------------------------------------------------
        public void SignMessage(string file)
        {
            string text = File.ReadAllText(file);
            BigInteger hash = Math.Abs(text.GetHashCode());

            BigInteger r, s;
            do
            {
                BigInteger k = PrimeUtils.RandomBigIntBetween(2, P - 2);
                while (BigInteger.GreatestCommonDivisor(k, P - 1) != 1)
                    k = PrimeUtils.RandomBigIntBetween(2, P - 2);

                r = BigInteger.ModPow(G, k, P);
                BigInteger kInv = ModInverse(k, P - 1);
                s = ((hash - X * r) * kInv) % (P - 1);
                if (s < 0) s += (P - 1);
            }
            while (r == 0 || s == 0);

            File.WriteAllText("Data/signature.txt", $"{r}\n{s}");
            if (DebugMode)
                Console.WriteLine("✅ Підпис створено у Data/signature.txt");
        }

        public void VerifySignature(string file)
        {
            if (!File.Exists("Data/signature.txt"))
            {
                Console.WriteLine("❌ Підпис не знайдено.");
                return;
            }

            string[] lines = File.ReadAllLines("Data/signature.txt");
            if (lines.Length < 2)
            {
                Console.WriteLine("❌ Неправильний формат файлу підпису.");
                return;
            }

            BigInteger r = BigInteger.Parse(lines[0]);
            BigInteger s = BigInteger.Parse(lines[1]);

            string text = File.ReadAllText(file);
            BigInteger hash = Math.Abs(text.GetHashCode());

            BigInteger v1 = (BigInteger.ModPow(Y, r, P) * BigInteger.ModPow(r, s, P)) % P;
            BigInteger v2 = BigInteger.ModPow(G, hash, P);

            if (v1 == v2)
                Console.WriteLine("✅ Підпис перевірено. Повідомлення справжнє.");
            else
                Console.WriteLine("❌ Підпис недійсний.");
        }

        // 🔹 Допоміжні методи ------------------------------------------------------
        private static BigInteger ModInverse(BigInteger a, BigInteger mod)
        {
            BigInteger m0 = mod, t, q;
            BigInteger x0 = 0, x1 = 1;
            if (mod == 1) return 0;

            while (a > 1)
            {
                q = a / mod;
                t = mod;
                mod = a % mod;
                a = t;
                t = x0;
                x0 = x1 - q * x0;
                x1 = t;
            }

            if (x1 < 0)
                x1 += m0;

            return x1;
        }

        private void GenerateKeys()
        {
            P = PrimeUtils.GeneratePrime(64);
            if (P < 5) P = 467;

            G = new BigInteger(2);
            X = PrimeUtils.RandomBigIntBetween(2, P - 2);
            Y = BigInteger.ModPow(G, X, P);
        }

        private void SaveKeys()
        {
            using (var sw = new StreamWriter(keysPath))
            {
                sw.WriteLine($"P={P}");
                sw.WriteLine($"G={G}");
                sw.WriteLine($"X={X}");
                sw.WriteLine($"Y={Y}");
            }
        }

        private void LoadKeys()
        {
            try
            {
                var lines = File.ReadAllLines(keysPath);
                P = BigInteger.Parse(lines[0].Split('=')[1]);
                G = BigInteger.Parse(lines[1].Split('=')[1]);
                X = BigInteger.Parse(lines[2].Split('=')[1]);
                Y = BigInteger.Parse(lines[3].Split('=')[1]);
            }
            catch
            {
                Console.WriteLine("⚠ Помилка читання ключів, створюємо нові...");
                GenerateKeys();
                SaveKeys();
            }
        }
    }
}
