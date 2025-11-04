using System;
using System.IO;

namespace SecureAuthSystem.Utils
{
    public static class Logger
    {
        private static readonly string LogFile = Path.Combine("Data", "us_book.txt");
        public static void Info(string user, string action, string details = "")
        {
            Directory.CreateDirectory("Data");
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}; user={user}; action={action}; details={details}";
            File.AppendAllText(LogFile, line + Environment.NewLine);
        }
    }
}
