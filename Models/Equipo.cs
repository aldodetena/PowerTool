using System.Windows.Media;
using System.ComponentModel;

namespace PowerTool.Models
{
    /// <summary>
    /// Representa un equipo dentro del dominio con propiedades como sistema operativo, estado de conexión, usuario actual, y direcciones IP/MAC.
    /// Implementa INotifyPropertyChanged para notificar cambios en las propiedades.
    /// </summary>
    public class Equipo : INotifyPropertyChanged
    {
        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string nombrePropiedad)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }

        // Propiedades con notificación de cambios
        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        private string _operatingSystem;
        public string OperatingSystem
        {
            get => _operatingSystem;
            set { _operatingSystem = value; OnPropertyChanged(nameof(OperatingSystem)); }
        }

        private string _operatingSystemVersion;
        public string OperatingSystemVersion
        {
            get => _operatingSystemVersion;
            set { _operatingSystemVersion = value; OnPropertyChanged(nameof(OperatingSystemVersion)); }
        }

        private DateTime _lastLogon;
        public DateTime LastLogon
        {
            get => _lastLogon;
            set { _lastLogon = value; OnPropertyChanged(nameof(LastLogon)); }
        }

        private Brush _isOnline;
        public Brush IsOnline
        {
            get => _isOnline;
            set { _isOnline = value; OnPropertyChanged(nameof(IsOnline)); }
        }

        private string _currentUser;
        public string CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(nameof(CurrentUser)); }
        }

        private string _ipAddress;
        public string IPAddress
        {
            get => _ipAddress;
            set { _ipAddress = value; OnPropertyChanged(nameof(IPAddress)); }
        }

        private string _macAddress;
        public string MACAddress
        {
            get => _macAddress;
            set { _macAddress = value; OnPropertyChanged(nameof(MACAddress)); }
        }

        public Equipo()
        {
            IsOnline = Brushes.Red;
            CurrentUser = "N/A";
            IPAddress = "N/A";
            MACAddress = "N/A";
        }
    }
}