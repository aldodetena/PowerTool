namespace PowerTool.Models
{
    public class RemoteProcess
    {
        public int ProcessId { get; set; }
        public string? Name { get; set; }
        public double CPUUsage { get; set; }
        public double MemoryUsage { get; set; }
    }
}
