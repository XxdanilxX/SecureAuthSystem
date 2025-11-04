using SecureAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecureAuthSystem.SystemCore
{
    public class AdminPanel
    {
        private List<User> users;
        private string usersFile;

        public AdminPanel(List<User> users, string usersFile)
        {
            this.users = users;
            this.usersFile = usersFile;
        }

        private void SaveUsers()
        {
            File.WriteAllLines(usersFile, users.Select(u => u.ToString()));
        }

        public void Show()
        {
            while (true)
            {
                Console.WriteLine("\n--- Панель адміністратора ---");
                Console.WriteLine("1 - Список користувачів");
                Console.WriteLine("2 - Додати користувача");
                Console.WriteLine("3 - Видалити користувача");
                Console.WriteLine("4 - Змінити роль користувача");
                Console.WriteLine("0 - Вихід з панелі");
                Console.Write("Вибір: ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        foreach (var u in users)
                            Console.WriteLine($"{u.Username} ({u.Role})");
                        break;

                    case "2":
                        Console.Write("Ім’я: ");
                        string n = Console.ReadLine();
                        Console.Write("Пароль: ");
                        string p = Console.ReadLine();
                        Console.Write("Роль (R/W/A/ADMIN): ");
                        string r = Console.ReadLine().ToUpper();
                        users.Add(new User { Username = n, Password = p, Role = r });
                        SaveUsers();
                        Console.WriteLine("✅ Користувача додано.");
                        break;

                    case "3":
                        Console.Write("Ім’я користувача для видалення: ");
                        string del = Console.ReadLine();
                        users.RemoveAll(u => u.Username == del);
                        SaveUsers();
                        Console.WriteLine("✅ Користувача видалено.");
                        break;

                    case "4":
                        Console.Write("Ім’я: ");
                        string name = Console.ReadLine();
                        var user = users.FirstOrDefault(u => u.Username == name);
                        if (user == null)
                        {
                            Console.WriteLine("❌ Не знайдено.");
                            break;
                        }
                        Console.Write("Нова роль (R/W/A/ADMIN): ");
                        user.Role = Console.ReadLine().ToUpper();
                        SaveUsers();
                        Console.WriteLine("✅ Роль змінено.");
                        break;

                    case "0":
                        return;
                }
            }
        }
    }
}
