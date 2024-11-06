namespace PowerTool.Models
{
    /// <summary>
    /// Contiene información de una entrada del visor de eventos remoto, como el origen, tipo de evento, tiempo generado y mensaje.
    /// </summary>
    public class RemoteEventLogEntry
    {
        public string? Source { get; set; }
        public string? EventType { get; set; }
        public DateTime TimeGenerated { get; set; }
        public string? Message { get; set; }
    }
}