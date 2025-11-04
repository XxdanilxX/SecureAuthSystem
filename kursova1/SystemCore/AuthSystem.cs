using SecureAuthSystem.Models;
using SecureAuthSystem.Security;
using SecureAuthSystem.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecureAuthSystem.SystemCore
{
    public class AuthSystem
    {
        private readonly string usersFileEnc = Path.Combine("Data", "nameuser.enc");
        private readonly string usersTempFile = Path.Combine("Data", "nameuser_temp.txt");
        private readonly string outFile = Path.Combine("Data", "out.txt");
        private readonly string inputFile = Path.Combine("Data", "input.txt");

        public List<User> Users { get; private set; } = new List<User>();

        private readonly ElGamal crypto = new ElGamal();

        public AuthSystem()
        {
            Directory.CreateDirectory("Data");

            // Якщо зашифрований файл користувачів ще не існує — створюємо адміністратора
            if (!File.Exists(usersFileEnc))
            {
                var admin = $"admin admin123 ADMIN {DateTime.Now:yyyy-MM-dd} 365 A:E;B:E;C:E;D:E;E:E";
                File.WriteAllText(usersTempFile, admin + Environment.NewLine);

                // Шифруємо базу користувачів
                crypto.EncryptFile(usersTempFile);

                if (File.Exists(usersFileEnc))
                    File.Delete(usersFileEnc);

                File.Move("Data/close.txt", usersFileEnc);
                File.Delete(usersTempFile);
            }

            // Розшифровуємо файл користувачів у тимчасовий файл
            crypto.DecryptToFile(usersFileEnc, usersTempFile);

            // Зчитуємо користувачів
            Users = File.ReadAllLines(usersTempFile)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(User.Parse)
                .ToList();

            // Видаляємо розшифрований тимчасовий файл
            File.Delete(usersTempFile);

            AccessControl.EnsureFolders();
            if (!File.Exists(inputFile))
                File.WriteAllText(inputFile, "Hello Secure World!");
        }

        // Зберігаємо користувачів назад у зашифрованому вигляді
        private void SaveUsers()
        {
            File.WriteAllLines(usersTempFile, Users.Select(u => u.ToString()));

            crypto.EncryptFile(usersTempFile);

            if (File.Exists(usersFileEnc))
                File.Delete(usersFileEnc);

            File.Move("Data/close.txt", usersFileEnc);
            File.Delete(usersTempFile);
        }

        public User Login()
        {
            Console.Write("Ім’я користувача: ");
            string uname = Console.ReadLine();
            Console.Write("Пароль: ");
            string pwd = Console.ReadLine();

            var u = Users.FirstOrDefault(x => x.Username == uname && x.Password == pwd);
            if (u == null)
            {
                Console.WriteLine("❌ Невірні дані.");
                return null;
            }

            if (u.IsPasswordExpired())
            {
                Console.WriteLine("⚠ Пароль прострочений. Змініть пароль:");
                Console.Write("Новий пароль: ");
                u.Password = Console.ReadLine();
                u.PasswordSetAt = DateTime.Now.Date;
                SaveUsers();
                Console.WriteLine("✅ Пароль оновлено.");
            }

            Logger.Info(u.Username, "login");
            Console.WriteLine($"✅ Вітаємо, {u.Username}! Роль: {u.Role}");
            AccessControl.ShowAvailable(u);
            return u;
        }

        public void Session(User user)
        {
            var adminPanel = new AdminPanel(Users, usersFileEnc);
            var ask = new AskEngine(8);
            ask.OnFail += () => { Logger.Info(user.Username, "auth_fail"); Environment.Exit(0); };
            ask.Start();
            while (true)
            {
                Console.WriteLine("\n--- Меню ---");
                Console.WriteLine("1 - Переглянути out.txt (розшифрований)");
                Console.WriteLine("2 - Редагувати/дописати input.txt (з подальшим шифруванням)");
                Console.WriteLine("3 - Шифрувати input.txt -> close.txt");
                Console.WriteLine("4 - Розшифрувати close.txt -> out.txt");
                Console.WriteLine("5 - Підписати out.txt та перевірити підпис");
                if (user.Role == "ADMIN") Console.WriteLine("9 - Адмін-панель");
                Console.WriteLine("0 - Вихід");
                Console.Write("Ваш вибір: ");
                var ch = Console.ReadLine();

                switch (ch)
                {
                    case "1":
                        if (File.Exists(outFile))
                            Console.WriteLine(File.ReadAllText(outFile));
                        else
                            Console.WriteLine("(Файл out.txt поки відсутній)");
                        Logger.Info(user.Username, "view_out");
                        break;

                    case "2":
                        if (user.Role == "R" || user.Role == "E")
                        {
                            Console.WriteLine("❌ Немає прав редагувати.");
                            break;
                        }

                        Console.Write("Введіть текст (ENTER завершить рядок): ");
                        string text = Console.ReadLine() + Environment.NewLine;

                        if (user.Role == "A")
                            File.AppendAllText(inputFile, text);
                        else
                            File.WriteAllText(inputFile, text);

                        System.Threading.Thread.Sleep(100);

                        if (new FileInfo(inputFile).Length == 0)
                        {
                            Console.WriteLine("⚠ input.txt порожній, нічого не зашифровано.");
                            break;
                        }

                        crypto.EncryptFile(inputFile);
                        Console.WriteLine("✅ Текст зашифровано і записано до close.txt");
                        Logger.Info(user.Username, "encrypt_done");
                        break;

                    case "3":
                        if (user.Role == "R")
                        {
                            Console.WriteLine("❌ Немає прав.");
                            break;
                        }
                        crypto.EncryptFile(inputFile);
                        Console.WriteLine("✅ Зашифровано.");
                        Logger.Info(user.Username, "encrypt");
                        break;

                    case "4":
                        string decrypted = crypto.DecryptFileToText("Data/close.txt");
                        Console.WriteLine("✅ Розшифрований текст:");
                        Console.WriteLine(decrypted);
                        break;

                    case "5":
                        crypto.SignMessage("Data/out.txt");
                        crypto.VerifySignature("Data/out.txt");
                        break;

                    case "9":
                        if (user.Role != "ADMIN")
                        {
                            Console.WriteLine("❌ Тільки адмін.");
                            break;
                        }
                        adminPanel.Show();
                        SaveUsers();
                        break;

                    case "0":
                        Logger.Info(user.Username, "logout");
                        return;
                }
            }
        }
    }
}
