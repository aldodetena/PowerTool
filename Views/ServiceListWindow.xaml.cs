using System.Management;
using System.Windows;
using PowerTool.Models;
using System.Collections.ObjectModel;
using PowerTool.Utilities;
using PowerTool.Services;

namespace PowerTool.Views
{
    /// <summary>
    /// Ventana que muestra una lista de servicios en ejecución en un equipo remoto
    /// y permite gestionar su estado (iniciar, detener, reiniciar).
    /// </summary>
    public partial class ServiceListWindow : Window
    {
        /// <summary>
        /// Información del dominio seleccionado utilizada para establecer conexiones remotas.
        /// </summary>
        private DomainInfo _selectedDomain;
        /// <summary>
        /// Nombre del equipo remoto del cual se gestionan los servicios.
        /// </summary>
        private string _nombreEquipo;
        /// <summary>
        /// Colección observable de servicios obtenidos del equipo remoto.
        /// Permite la actualización dinámica en la interfaz de usuario.
        /// </summary>
        public ObservableCollection<ServiceInfo> Servicios { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="ServiceListWindow"/> con la lista de servicios del equipo remoto.
        /// </summary>
        /// <param name="servicios">Lista de servicios obtenidos del equipo remoto.</param>
        /// <param name="nombreEquipo">Nombre del equipo remoto.</param>
        /// <param name="selectedDomain">Información del dominio para la conexión remota.</param>
        public ServiceListWindow(List<ServiceInfo> servicios, string nombreEquipo, DomainInfo selectedDomain)
        {
            InitializeComponent();
            _selectedDomain = selectedDomain;
            _nombreEquipo = nombreEquipo;
            Servicios = new ObservableCollection<ServiceInfo>(servicios);
            DataContext = this;
        }

        /// <summary>
        /// Inicia el servicio seleccionado en el equipo remoto.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void IniciarServicio_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceListView.SelectedItem is ServiceInfo servicioSeleccionado)
            {
                CambiarEstadoServicio(servicioSeleccionado, "StartService");
                ActualizarEstadoServicio(servicioSeleccionado); // Actualizar después de iniciar
            }
        }

        /// <summary>
        /// Detiene el servicio seleccionado en el equipo remoto.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void DetenerServicio_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceListView.SelectedItem is ServiceInfo servicioSeleccionado)
            {
                CambiarEstadoServicio(servicioSeleccionado, "StopService");
                ActualizarEstadoServicio(servicioSeleccionado); // Actualizar después de detener
            }
        }

        /// <summary>
        /// Reinicia el servicio seleccionado en el equipo remoto.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void ReiniciarServicio_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceListView.SelectedItem is ServiceInfo servicioSeleccionado)
            {
                CambiarEstadoServicio(servicioSeleccionado, "StopService");
                CambiarEstadoServicio(servicioSeleccionado, "StartService");
                ActualizarEstadoServicio(servicioSeleccionado); // Actualizar después de reiniciar
            }
        }

        /// <summary>
        /// Cambia el estado de un servicio remoto invocando un método WMI.
        /// </summary>
        /// <param name="servicio">Información del servicio a gestionar.</param>
        /// <param name="metodo">Nombre del método WMI a invocar (StartService, StopService, etc.).</param>
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

        /// <summary>
        /// Actualiza el estado de un servicio remoto en la lista después de un cambio.
        /// </summary>
        /// <param name="servicio">Información del servicio a actualizar.</param>
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
