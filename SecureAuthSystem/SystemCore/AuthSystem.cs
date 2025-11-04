using SecureAuthSystem.Models;
using SecureAuthSystem.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecureAuthSystem.SystemCore
{
    public class AuthSystem
    {
        private const string UsersFile = "Data/nameuser.txt";
        private const string EncryptedFile = "Data/close.txt";
        private const string LogFile = "Data/us_book.txt";

        public List<User> Users { get; private set; } = new List<User>();
        private ElGamal crypto = new ElGamal();

        public AuthSystem()
        {
            Directory.CreateDirectory("Data");
            LoadUsers();
        }

        private void LoadUsers()
        {
            if (!File.Exists(UsersFile))
            {
                File.WriteAllText(UsersFile, "admin admin123 ADMIN\n");
            }
            Users = File.ReadAllLines(UsersFile)
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .Select(l =>
                        {
                            var parts = l.Split(' ');
                            return new User { Username = parts[0], Password = parts[1], Role = parts[2] };
                        }).ToList();
        }

        public User Login()
        {
            Console.Write("Ім’я користувача: ");
            string username = Console.ReadLine();
            Console.Write("Пароль: ");
            string password = Console.ReadLine();

            var user = Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user == null)
            {
                Console.WriteLine("❌ Невірне ім’я або пароль.");
                return null;
            }

            File.AppendAllText(LogFile, $"{DateTime.Now}: {user.Username} увійшов у систему\n");
            Console.WriteLine($"✅ Вітаємо, {user.Username}! Ваша роль: {user.Role}");
            return user;
        }

        public void UserPanel(User user)
        {
            AdminPanel adminPanel = new AdminPanel(Users, UsersFile);

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\n--- Меню користувача ---");
                Console.WriteLine("1 - Переглянути розшифрований текст");
                if (user.Role == "W" || user.Role == "A" || user.Role == "ADMIN")
                    Console.WriteLine("2 - Редагувати (зашифрувати) текст");
                if (user.Role == "ADMIN")
                    Console.WriteLine("3 - Панель адміністратора");
                Console.WriteLine("0 - Вихід");

                Console.Write("Ваш вибір: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\n--- Розшифрований текст ---");
                        Console.WriteLine(crypto.DecryptFileToText(EncryptedFile));
                        break;

                    case "2":
                        if (user.Role == "R")
                        {
                            Console.WriteLine("❌ Ви не маєте права редагувати.");
                            break;
                        }
                        Console.Write("Введіть новий текст: ");
                        string text = Console.ReadLine();
                        crypto.EncryptTextToFile(text, EncryptedFile);
                        Console.WriteLine("✅ Текст зашифровано та збережено у close.txt");
                        break;

                    case "3":
                        if (user.Role == "ADMIN")
                            adminPanel.Show();
                        else
                            Console.WriteLine("❌ Недостатньо прав.");
                        break;

                    case "0":
                        exit = true;
                        break;
                }
            }
        }
    }
}
