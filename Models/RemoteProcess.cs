namespace PowerTool.Models
{
    /// <summary>
    /// Representa un proceso remoto con información como el ID del proceso, nombre, uso de CPU y uso de memoria.
    /// </summary>
    public class RemoteProcess
    {
        public int ProcessId { get; set; }
        public string? Name { get; set; }
        public double CPUUsage { get; set; }
        public double MemoryUsage { get; set; }
    }
}
