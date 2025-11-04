using SecureAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SecureAuthSystem.SystemCore
{
    public static class AccessControl
    {
        private static readonly char[] Disks = new[] { 'A', 'B', 'C', 'D', 'E' };

        public static void EnsureFolders()
        {
            foreach (var d in Disks)
            {
                Directory.CreateDirectory(Path.Combine("Data", d.ToString()));
            }
        }

        public static void ShowAvailable(User u)
        {
            Console.WriteLine("Доступні каталоги:");
            foreach (var d in Disks)
            {
                char r = u.DiskRights.ContainsKey(d) ? u.DiskRights[d] : '-';
                if (r != '-') Console.WriteLine($"  {d}: {r}");
            }
        }

        public static bool HasAtLeast(User u, char disk, char need)
        {
            if (!u.DiskRights.TryGetValue(disk, out char have)) return false;
            if (have == '-') return false;
            if (need == 'R') return have == 'R' || have == 'W' || have == 'A' || have == 'E' || u.Role == "ADMIN";
            if (need == 'W') return have == 'W' || u.Role == "ADMIN";
            if (need == 'A') return have == 'A' || u.Role == "ADMIN";
            if (need == 'E') return have == 'E' || u.Role == "ADMIN";
            return false;
        }
    }
}
