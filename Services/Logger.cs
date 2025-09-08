// Logger.cs  (добавьте в проект один раз)
using System;
using System.IO;

namespace Ynost.Services
{
    internal static class Logger
    {   
        private static readonly string _path =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ynost.log");

        public static void Write(string message)
        {
            try
            {
                File.AppendAllText(_path,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}");
            }
            catch { /* не ломаем программу из-за проблем с логированием */ }
        }

        public static void Write(Exception ex, string tag = "EX")
            => Write($"[{tag}] {ex}");
    }
}
