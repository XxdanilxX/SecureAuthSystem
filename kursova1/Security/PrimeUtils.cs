using System;
using System.Numerics;

namespace SecureAuthSystem.Security
{
    public static class PrimeUtils
    {
        private static readonly Random rnd = new Random();
        public static BigInteger GeneratePrime(int bits)
        {
            if (bits < 8) bits = 8;
            BigInteger result;
            do
            {
                result = RandomBigInt(bits);
                if (result < 0) result = BigInteger.Abs(result);
                if (result % 2 == 0) result += 1;
            } while (!IsProbablyPrime(result, 10));
            return result;
        }
        public static BigInteger RandomBigInt(int bits)
        {
            int bytes = (bits + 7) / 8;
            byte[] arr = new byte[bytes];
            rnd.NextBytes(arr);
            arr[arr.Length - 1] &= (byte)((1 << (bits % 8 == 0 ? 8 : bits % 8)) - 1);
            arr[arr.Length - 1] |= 0x80; // set top bit
            arr[0] |= 1; // odd
            return new BigInteger(arr);
        }
        public static bool IsProbablyPrime(BigInteger value, int witnesses = 10)
        {
            if (value <= 1) return false;
            if (value == 2) return true;
            if (value % 2 == 0) return false;

            BigInteger d = value - 1;
            int s = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                s++;
            }

            var rng = new Random();

            for (int i = 0; i < witnesses; i++)
            {
                // Створюємо випадкове a без переходу до Int32
                byte[] bytes = value.ToByteArray();
                BigInteger a;
                do
                {
                    rng.NextBytes(bytes);
                    a = new BigInteger(bytes);
                    a = BigInteger.Abs(a % (value - 3)) + 2;
                }
                while (a < 2 || a >= value - 2);

                BigInteger x = BigInteger.ModPow(a, d, value);
                if (x == 1 || x == value - 1)
                    continue;

                bool cont = false;
                for (int r = 0; r < s - 1; r++)
                {
                    x = BigInteger.ModPow(x, 2, value);
                    if (x == 1) return false;
                    if (x == value - 1)
                    {
                        cont = true;
                        break;
                    }
                }

                if (!cont)
                    return false;
            }
            return true;
        }

        public static BigInteger RandomBigIntBetween(BigInteger min, BigInteger max)
        {
            if (min >= max) return min + 1;

            var diff = max - min;
            byte[] bytes = diff.ToByteArray();
            BigInteger r;

            do
            {
                rnd.NextBytes(bytes);
                r = new BigInteger(bytes);
                r = BigInteger.Abs(r);
            }
            while (r == 0 || r >= diff);

            return min + r;
        }


        // Miller–Rabin
        public static bool IsProbablePrime(BigInteger n, int k = 16)
        {
            if (n < 2) return false;
            int[] small = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };
            foreach (int p in small)
            {
                if (n == p) return true;
                if (n % p == 0) return false;
            }

            BigInteger d = n - 1;
            int s = 0;
            while ((d & 1) == 0) { d >>= 1; s++; }

            for (int i = 0; i < k; i++)
            {
                BigInteger a = RandomBigIntBetween(2, n - 2);
                BigInteger x = BigInteger.ModPow(a, d, n);
                if (x == 1 || x == n - 1) continue;
                bool cont = false;
                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == n - 1) { cont = true; break; }
                }
                if (!cont) return false;
            }
            return true;
        }

        // генерує безпечне просте p = 2q+1 приблизно потрібної кількості десяткових знаків
        public static BigInteger GenerateSafePrimeWithDigits(int decimalDigits)
        {
            // 1 десяткова ~ log10(2)≈0.301 -> bits ≈ digits / 0.301
            int bits = (int)(decimalDigits / 0.30103) + 2;
            while (true)
            {
                BigInteger q;
                do
                {
                    q = RandomBigInt(bits - 1);
                } while (!IsProbablePrime(q));

                BigInteger p = 2 * q + 1;
                if (IsProbablePrime(p) && p.ToString().Length >= decimalDigits)
                    return p;
            }
        }

        public static BigInteger ModInverse(BigInteger a, BigInteger mod)
        {
            BigInteger t = 0, newT = 1;
            BigInteger r = mod, newR = a % mod;
            while (newR != 0)
            {
                BigInteger q = r / newR;
                (t, newT) = (newT, t - q * newT);
                (r, newR) = (newR, r - q * newR);
            }
            if (r > 1) throw new Exception("No inverse");
            if (t < 0) t += mod;
            return t;
        }
    }
}
