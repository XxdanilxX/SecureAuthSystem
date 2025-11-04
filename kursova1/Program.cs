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
            var system = new AuthSystem();

            while (true)
            {
                Console.WriteLine("\n=== Система ідентифікації та шифрування ===");
                Console.WriteLine("1 - Вхід до системи");
                Console.WriteLine("0 - Вихід");
                Console.Write("Вибір: ");
                var ch = Console.ReadLine();
                if (ch == "0") return;

                if (ch == "1")
                {
                    var u = system.Login();
                    if (u != null) system.Session(u);
                }
            }
        }
    }
}
