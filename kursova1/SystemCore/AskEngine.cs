using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace SecureAuthSystem.SystemCore
{
    public class AskEngine
    {
        private readonly string file = Path.Combine("Data", "ask.txt");
        private readonly int periodSec;
        private readonly Random rnd = new Random();
        private Timer timer;
        private List<double> xs = new List<double>();
        public event Action OnFail; // якщо відповів неправильно

        public AskEngine(int periodSeconds = 8)
        {
            periodSec = periodSeconds;
            EnsureFile();
            xs = File.ReadAllLines(file)
                     .Where(s => double.TryParse(s.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                             System.Globalization.CultureInfo.InvariantCulture, out _))
                     .Select(s => double.Parse(s.Replace(',', '.'),
                             System.Globalization.CultureInfo.InvariantCulture))
                     .ToList();
            if (xs.Count == 0) xs.AddRange(new[] { 0.5, 1.2, 2.3, 3.7, 4.1, 5.9 });
        }

        private void EnsureFile()
        {
            Directory.CreateDirectory("Data");
            if (!File.Exists(file))
            {
                File.WriteAllLines(file, new[] { "0.5", "1.2", "2.3", "3.7", "4.1", "5.9" });
            }
        }

        public void Start()
        {
            timer = new Timer(periodSec * 1000);
            timer.Elapsed += (s, e) => AskOnce();
            timer.AutoReset = true;
            timer.Start();
        }

        public void Stop() => timer?.Stop();

        private void AskOnce()
        {
            double x = xs[rnd.Next(xs.Count)];
            double correct = 4.0 * Math.Sin(x);
            Console.WriteLine($"\n[Автентифікація] Обчисліть Y = 4 * sin({x:F3}). Ваша відповідь?: ");
            string input = Console.ReadLine();
            if (!double.TryParse(input.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                 System.Globalization.CultureInfo.InvariantCulture, out double y))
            {
                Console.WriteLine("Невірний формат.");
                OnFail?.Invoke();
                return;
            }
            if (Math.Abs(y - correct) > 0.01)
            {
                Console.WriteLine("❌ Неправильно. Сесію завершено.");
                OnFail?.Invoke();
            }
            else
            {
                Console.WriteLine("✅ Вірно. Продовжуємо роботу.");
            }
        }
    }
}
