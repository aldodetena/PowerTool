using System.ComponentModel;

namespace PowerTool.Models
{
    /// <summary>
    /// Representa una cuenta de usuario en el dominio con información sobre el estado, último inicio de sesión, y si está habilitada o bloqueada.
    /// Implementa INotifyPropertyChanged para manejar cambios en las propiedades.
    /// </summary>
    public class UserAccount : INotifyPropertyChanged
    {
        private string? _status;
        private bool _isEnabled;
        private bool _isLocked;

        public string? Name { get; set; }
        public string? LastLogin { get; set; }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                _isLocked = value;
                OnPropertyChanged(nameof(IsLocked));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
