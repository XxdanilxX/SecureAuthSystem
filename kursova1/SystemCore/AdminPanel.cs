using SecureAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecureAuthSystem.SystemCore
{
    public class AdminPanel
    {
        private readonly List<User> users;
        private readonly string usersFile;
        private const int MaxUsers = 14; // N=14

        public AdminPanel(List<User> users, string file)
        {
            this.users = users;
            usersFile = file;
        }

        private void Save() => File.WriteAllLines(usersFile, users.Select(u => u.ToString()));

        public void Show()
        {
            while (true)
            {
                Console.WriteLine("\n--- Адмін-панель ---");
                Console.WriteLine("1 - Список користувачів");
                Console.WriteLine("2 - Додати користувача");
                Console.WriteLine("3 - Видалити користувача");
                Console.WriteLine("4 - Змінити роль/права/термін пароля");
                Console.WriteLine("0 - Назад");
                Console.Write("Вибір: ");
                var ch = Console.ReadLine();

                switch (ch)
                {
                    case "1":
                        foreach (var u in users)
                            Console.WriteLine($"{u.Username} | {u.Role} | до {u.PasswordSetAt.AddDays(u.PasswordValidDays):yyyy-MM-dd} | rights: A:{u.DiskRights['A']},B:{u.DiskRights['B']},C:{u.DiskRights['C']},D:{u.DiskRights['D']},E:{u.DiskRights['E']}");
                        break;
                    case "2":
                        if (users.Count >= MaxUsers) { Console.WriteLine("Досягнуто ліміт N=14."); break; }
                        var nu = new User();
                        Console.Write("Ім’я: "); nu.Username = Console.ReadLine();
                        Console.Write("Пароль: "); nu.Password = Console.ReadLine();
                        Console.Write("Роль (ADMIN/R/W/A/E): "); nu.Role = Console.ReadLine().ToUpper();
                        nu.PasswordSetAt = DateTime.Now.Date;
                        Console.Write("Днів дії пароля (наприклад 90): "); nu.PasswordValidDays = int.Parse(Console.ReadLine());
                        foreach (var d in new[] { 'A', 'B', 'C', 'D', 'E' })
                        {
                            Console.Write($"{d} права (-/R/W/A/E): ");
                            nu.DiskRights[d] = Console.ReadLine().ToUpper()[0];
                        }
                        users.Add(nu); Save();
                        Console.WriteLine("✅ Додано.");
                        break;
                    case "3":
                        Console.Write("Кого видалити: ");
                        string del = Console.ReadLine();
                        users.RemoveAll(u => u.Username == del);
                        Save();
                        Console.WriteLine("✅ Видалено.");
                        break;
                    case "4":
                        Console.Write("Ім’я: ");
                        string name = Console.ReadLine();
                        var u2 = users.FirstOrDefault(u => u.Username == name);
                        if (u2 == null) { Console.WriteLine("Не знайдено."); break; }
                        Console.Write($"Нова роль (тепер {u2.Role}): "); var r = Console.ReadLine(); if (!string.IsNullOrWhiteSpace(r)) u2.Role = r.ToUpper();
                        Console.Write($"Новий термін пароля в днях (тепер {u2.PasswordValidDays}): "); var s = Console.ReadLine(); if (int.TryParse(s, out int days)) u2.PasswordValidDays = days;
                        foreach (var d in new[] { 'A', 'B', 'C', 'D', 'E' })
                        {
                            Console.Write($"{d} права (тепер {u2.DiskRights[d]}): ");
                            var inp = Console.ReadLine();
                            if (!string.IsNullOrWhiteSpace(inp)) u2.DiskRights[d] = inp.ToUpper()[0];
                        }
                        Save(); Console.WriteLine("✅ Оновлено.");
                        break;
                    case "0":
                        return;
                }
            }
        }
    }
}
