using System;

namespace PowerTool.Models
{
    public class Equipo
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? OperatingSystem { get; set; }
        public string? OperatingSystemVersion { get; set; }
        public DateTime LastLogonTimestamp { get; set; }
        public bool IsOnline { get; set; }
        public string? CurrentUser { get; set; }

        public Equipo()
        {
            IsOnline = false;
            CurrentUser = "N/A";
        }
    }
}