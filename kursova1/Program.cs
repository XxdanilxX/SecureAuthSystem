using SecureAuthSystem.Models;
using SecureAuthSystem.SystemCore;
using System;

namespace SecureAuthSystem
{
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            AuthSystem system = new AuthSystem();

            while (true)
            {
                Console.WriteLine("\n=== Система ідентифікації та шифрування ===");
                Console.WriteLine("1 - Вхід до системи");
                Console.WriteLine("0 - Вихід з програми");
                Console.Write("Вибір: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        User user = system.Login();
                        if (user != null)
                            system.UserPanel(user);
                        break;

                    case "0":
                        Console.WriteLine("Вихід...");
                        return;

                    default:
                        Console.WriteLine("Невірний вибір.");
                        break;
                }
            }
        }
    }
}
