using System.DirectoryServices;
using System.Net.NetworkInformation;
using System.Management;
using System.Windows;
using PowerTool.Models;  // Importar la clase Equipo
using PowerTool.Utilities;  // Importar la clase Logger
using System.Windows.Media;
using System.Windows.Controls;

namespace PowerTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Mostrar la ventana de diálogo para introducir el dominio
            DomainWindow domainWindow = new DomainWindow();
            if (domainWindow.ShowDialog() == true)
            {
                string dominio = domainWindow.DomainName;
                CargarEquiposDelDominio(dominio);
            }
            else
            {
                MessageBox.Show("No se ha introducido un dominio válido. La aplicación se cerrará.");
                this.Close();
            }
        }

        private void CargarEquiposDelDominio(string dominio)
        {
            try
            {
                DirectoryEntry entry = new DirectoryEntry($"LDAP://{dominio}");
                DirectorySearcher searcher = new DirectorySearcher(entry)
                {
                    Filter = "(objectCategory=computer)"
                };

                searcher.PropertiesToLoad.Add("name");
                searcher.PropertiesToLoad.Add("description");
                searcher.PropertiesToLoad.Add("operatingSystem");
                searcher.PropertiesToLoad.Add("operatingSystemVersion");
                searcher.PropertiesToLoad.Add("lastLogonTimestamp");

                SearchResultCollection resultados = searcher.FindAll();
                List<Equipo> equipos = new List<Equipo>();

                foreach (SearchResult resultado in resultados)
                {
                    string? nombre = resultado.Properties["name"].Count > 0 ? resultado.Properties["name"][0].ToString() : "";
                    string? descripcion = resultado.Properties["description"].Count > 0 ? resultado.Properties["description"][0].ToString() : "";
                    string? sistemaOperativo = resultado.Properties["operatingSystem"].Count > 0 ? resultado.Properties["operatingSystem"][0].ToString() : "";
                    string? versionSO = resultado.Properties["operatingSystemVersion"].Count > 0 ? resultado.Properties["operatingSystemVersion"][0].ToString() : "";
                    DateTime lastLogon = resultado.Properties["lastLogonTimestamp"].Count > 0 ? DateTime.FromFileTime((long)resultado.Properties["lastLogonTimestamp"][0]) : DateTime.MinValue;

                    equipos.Add(new Equipo
                    {
                        Name = nombre,
                        Description = descripcion,
                        OperatingSystem = sistemaOperativo,
                        OperatingSystemVersion = versionSO,
                        LastLogonTimestamp = lastLogon,
                        IsOnline = EstaEncendido(nombre),
                        CurrentUser = ObtenerUsuarioActual(nombre)
                    });
                }

                EquiposListView.ItemsSource = equipos;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al cargar los equipos del dominio {dominio}", ex);
            }
        }

        private bool EstaEncendido(string nombreEquipo)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(nombreEquipo);

                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private string ObtenerUsuarioActual(string nombreEquipo)
        {
            try
            {
                ManagementScope scope = new ManagementScope($@"\\{nombreEquipo}\root\cimv2");
                scope.Connect();

                ObjectQuery query = new ObjectQuery("SELECT UserName FROM Win32_ComputerSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

                foreach (ManagementObject result in searcher.Get())
                {
                    return result["UserName"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al obtener el usuario actual del equipo {nombreEquipo}", ex);
            }
            return "N/A";
        }

        private void AbrirPopUpComando_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Equipo equipoSeleccionado)
            {
                // Crear y mostrar un PopUp para ingresar el comando
                CommandWindow commandWindow = new CommandWindow(equipoSeleccionado);
                commandWindow.ShowDialog(); // Mostrar el PopUp de forma modal
            }
        }

        private void ConectarRDPButton_Click(object sender, RoutedEventArgs e)
        {
            if (EquiposListView.SelectedItem is Equipo equipoSeleccionado)
            {
                try
                {
                    // Lanza la aplicación de escritorio remoto (RDP)
                    System.Diagnostics.Process.Start("mstsc", $"/v:{equipoSeleccionado.Name}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error al intentar conectarse al equipo {equipoSeleccionado.Name} mediante RDP", ex);
                    MessageBox.Show($"Error al intentar conectarse al equipo {equipoSeleccionado.Name} mediante RDP: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un equipo de la lista.");
            }
        }

        private void CerrarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}