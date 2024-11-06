using System.IO;

namespace PowerTool.Utilities
{
    /// <summary>
    /// Proporciona funcionalidad para registrar mensajes de error en un archivo de log.
    /// </summary>
    public static class Logger
    {
        private static readonly string logFilePath = "PowerToolLogs.log";

        /// <summary>
        /// Registra un mensaje de error junto con la excepción en un archivo de log.
        /// </summary>
        /// <param name="message">El mensaje de error descriptivo.</param>
        /// <param name="ex">La excepción asociada al error.</param>
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