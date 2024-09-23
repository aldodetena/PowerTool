using System.Collections.ObjectModel;
using System.Windows.Data;
using System.DirectoryServices;
using System.Net.NetworkInformation;
using System.Management;
using System.Windows;
using PowerTool.Models;  // Importar la clase Equipo
using PowerTool.Utilities;  // Importar la clase Logger
using SkiaSharp;
using Svg.Skia;
using SkiaSharp.Views.Desktop;
using System.IO;
using System.Windows.Controls;

namespace PowerTool
{
    public partial class MainWindow : Window
    {
        private readonly SKSvg svgComputer;
        private readonly SKSvg svgScript;
        private readonly SKSvg svgRemote;
        private ObservableCollection<Equipo> equipos;

        public MainWindow()
        {
            InitializeComponent();

             // Cargar los SVGs
            svgComputer = new SKSvg();
            svgComputer.Load(Path.Combine("icons", "computer.svg"));

            svgScript = new SKSvg();
            svgScript.Load(Path.Combine("icons", "script.svg"));

            svgRemote = new SKSvg();
            svgRemote.Load(Path.Combine("icons", "remote.svg"));

            equipos = new ObservableCollection<Equipo>();
            EquiposListView.ItemsSource = equipos;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(EquiposListView.ItemsSource);
            view.Filter = EquipoFilter;

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

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(EquiposListView.ItemsSource).Refresh();
        }
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svgComputer.Picture);
        }

        private void OnPaintSurfaceScript(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svgScript.Picture);
        }

        private void OnPaintSurfaceRemote(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svgRemote.Picture);
        }

        private bool EquipoFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            return (item as Equipo).Name.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private async void CargarEquiposDelDominio(string dominio)
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
                List<Task<Equipo>> tareas = new List<Task<Equipo>>();

                foreach (SearchResult resultado in resultados)
                {
                    tareas.Add(Task.Run(() =>
                    {
                        string nombre = resultado.Properties["name"].Count > 0 ? resultado.Properties["name"][0].ToString() : "";
                        string descripcion = resultado.Properties["description"].Count > 0 ? resultado.Properties["description"][0].ToString() : "";
                        string sistemaOperativo = resultado.Properties["operatingSystem"].Count > 0 ? resultado.Properties["operatingSystem"][0].ToString() : "";
                        string versionSO = resultado.Properties["operatingSystemVersion"].Count > 0 ? resultado.Properties["operatingSystemVersion"][0].ToString() : "";
                        DateTime lastLogon = resultado.Properties["lastLogonTimestamp"].Count > 0 ? DateTime.FromFileTime((long)resultado.Properties["lastLogonTimestamp"][0]) : DateTime.MinValue;

                        var equipo = new Equipo
                        {
                            Name = nombre,
                            Description = descripcion,
                            OperatingSystem = sistemaOperativo,
                            OperatingSystemVersion = versionSO,
                            LastLogonTimestamp = lastLogon,
                            IsOnline = EstaEncendido(nombre),
                            CurrentUser = ObtenerUsuarioActual(nombre)
                        };

                        return equipo;
                    }));
                }

                Equipo[] equiposCargados = await Task.WhenAll(tareas);
                foreach (var equipo in equiposCargados)
                {
                    equipos.Add(equipo);
                }
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