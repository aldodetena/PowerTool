using System.IO;

namespace PowerTool.Utilities
{
    public static class Logger
    {
        private static readonly string logFilePath = "PowerToolLogs.log";

        public static void LogError(string message, Exception ex)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                    writer.WriteLine($"Exception: {ex.Message}");
                    writer.WriteLine($"Stack Trace: {ex.StackTrace}");
                    writer.WriteLine("--------------------------------------------------");
                }
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Error al escribir en el log: {logEx.Message}");
            }
        }
    }
}