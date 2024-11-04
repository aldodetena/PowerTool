using System.Management;
using System.Windows;
using PowerTool.Models;
using System.Collections.ObjectModel;
using PowerTool.Utilities;
using PowerTool.Services;

namespace PowerTool.Views
{
    public partial class ServiceListWindow : Window
    {
        private DomainInfo _selectedDomain;
        private string _nombreEquipo;
        public ObservableCollection<ServiceInfo> Servicios { get; set; }

        public ServiceListWindow(List<ServiceInfo> servicios, string nombreEquipo, DomainInfo selectedDomain)
        {
            InitializeComponent();
            _selectedDomain = selectedDomain;
            _nombreEquipo = nombreEquipo;
            Servicios = new ObservableCollection<ServiceInfo>(servicios);
            DataContext = this;
        }

        private void IniciarServicio_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceListView.SelectedItem is ServiceInfo servicioSeleccionado)
            {
                CambiarEstadoServicio(servicioSeleccionado, "StartService");
                ActualizarEstadoServicio(servicioSeleccionado); // Actualizar después de iniciar
            }
        }

        private void DetenerServicio_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceListView.SelectedItem is ServiceInfo servicioSeleccionado)
            {
                CambiarEstadoServicio(servicioSeleccionado, "StopService");
                ActualizarEstadoServicio(servicioSeleccionado); // Actualizar después de detener
            }
        }

        private void ReiniciarServicio_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceListView.SelectedItem is ServiceInfo servicioSeleccionado)
            {
                CambiarEstadoServicio(servicioSeleccionado, "StopService");
                CambiarEstadoServicio(servicioSeleccionado, "StartService");
                ActualizarEstadoServicio(servicioSeleccionado); // Actualizar después de reiniciar
            }
        }

        private void CambiarEstadoServicio(ServiceInfo servicio, string metodo)
        {
            try
            {
                string password = EncryptionHelper.DecryptString(_selectedDomain.EncryptedPassword);
                var options = new ConnectionOptions
                {
                    Username = _selectedDomain.Username,
                    Password = password,
                    Impersonation = ImpersonationLevel.Impersonate,
                    Authentication = AuthenticationLevel.PacketPrivacy
                };
                var scope = new ManagementScope($@"\\{_nombreEquipo}\root\cimv2", options);
                scope.Connect();

                var service = new ManagementObject(scope, new ManagementPath($"Win32_Service.Name='{servicio.Name}'"), null);
                service.InvokeMethod(metodo, null);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al intentar {metodo} el servicio {servicio.Name} en {_nombreEquipo}", ex);
                MessageBox.Show($"Error al intentar {metodo} el servicio. Consulte el log para más detalles.");
            }
        }

        private void ActualizarEstadoServicio(ServiceInfo servicio)
        {
            try
            {
                string password = EncryptionHelper.DecryptString(_selectedDomain.EncryptedPassword);
                var options = new ConnectionOptions
                {
                    Username = _selectedDomain.Username,
                    Password = password
                };
                var scope = new ManagementScope($@"\\{_nombreEquipo}\root\cimv2", options);
                scope.Connect();

                var query = new ObjectQuery($"SELECT State FROM Win32_Service WHERE Name='{servicio.Name}'");
                var searcher = new ManagementObjectSearcher(scope, query);
                var result = searcher.Get();

                foreach (ManagementObject mo in result)
                {
                    servicio.State = mo["State"]?.ToString(); // Actualiza el estado del servicio
                }

                // Refresca la lista para mostrar el estado actualizado en la interfaz
                ServiceListView.Items.Refresh();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al actualizar el estado del servicio {servicio.Name} en {_nombreEquipo}", ex);
                MessageBox.Show($"Error al actualizar el estado del servicio. Consulte el log para más detalles.");
            }
        }
    }
}
