using System.Windows.Media;

namespace PowerTool.Models
{
    public class Equipo
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? OperatingSystem { get; set; }
        public string? OperatingSystemVersion { get; set; }
        public DateTime LastLogon { get; set; }
        public Brush IsOnline { get; set; }
        public string? CurrentUser { get; set; }
        public string IPAddress { get; set; }
        public string MACAddress { get; set; }

        public Equipo()
        {
            IsOnline = Brushes.Red;
            CurrentUser = "N/A";
            IPAddress = "N/A";
            MACAddress = "N/A";
        }
    }
}