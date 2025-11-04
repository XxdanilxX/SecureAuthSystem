using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureAuthSystem.Models
{
    // Права по каталогу: -, R, W, A, E
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }        // у ТЗ — зберігається у відкритому вигляді
        public string Role { get; set; }            // ADMIN / R / W / A / E
        public DateTime PasswordSetAt { get; set; } // безпечний час
        public int PasswordValidDays { get; set; }  // скільки дійсний пароль
        public Dictionary<char, char> DiskRights { get; set; } = new Dictionary<char, char>
        { ['A'] = '-', ['B'] = '-', ['C'] = '-', ['D'] = '-', ['E'] = '-' };

        // формат зберігання в nameuser.txt
        // username password role yyyy-MM-dd validDays A:R;B:W;C:-;D:R;E:E
        public override string ToString()
        {
            string when = PasswordSetAt.ToString("yyyy-MM-dd");
            string rights = string.Join(";", DiskRights.Select(kv => $"{kv.Key}:{kv.Value}"));
            return $"{Username} {Password} {Role} {when} {PasswordValidDays} {rights}";
        }

        public static User Parse(string line)
        {
            var parts = line.Split(' ');
            if (parts.Length < 3)
                throw new FormatException("Неправильний формат рядка користувача: " + line);

            var user = new User
            {
                Username = parts[0],
                Password = parts[1],
                Role = parts[2],
                PasswordSetAt = DateTime.Now.Date,
                PasswordValidDays = 90
            };

            // Якщо є дата і кількість днів
            if (parts.Length >= 5)
            {
                DateTime tmpDate;
                if (DateTime.TryParse(parts[3], out tmpDate))
                    user.PasswordSetAt = tmpDate;
                int tmpInt;
                if (int.TryParse(parts[4], out tmpInt))
                    user.PasswordValidDays = tmpInt;
            }

            // Якщо є права доступу (6-й елемент)
            if (parts.Length >= 6)
            {
                var rights = parts[5].Split(';');
                foreach (var r in rights)
                {
                    var kv = r.Split(':');
                    if (kv.Length == 2 && kv[0].Length == 1 && kv[1].Length == 1)
                        user.DiskRights[kv[0][0]] = kv[1][0];
                }
            }
            else
            {
                // якщо немає — задати стандартні права
                var keys = new List<char>(user.DiskRights.Keys);
                foreach (var d in keys)
                    user.DiskRights[d] = 'E';
            }

            return user;
        }


        public bool IsPasswordExpired() =>
            DateTime.Now.Date > PasswordSetAt.Date.AddDays(PasswordValidDays);
    }
}
