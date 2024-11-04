namespace PowerTool.Models
{
    public class HardwareInfo
    {
        public string? MachineName { get; set; }
        public string? Cpu { get; set; }
        public string? CpuDetails { get; set; }
        public double RamInGB { get; set; }
        public double DiskSpaceInGB { get; set; }
        public double DiskFreeSpaceInGB { get; set; }
    }
}
