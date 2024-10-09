using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Data;
using System.DirectoryServices;
using System.Net.NetworkInformation;
using System.Management;
using System.Windows;
using PowerTool.Models;
using PowerTool.Utilities;
using PowerTool.Services;
using PowerTool.Views;
using SkiaSharp;
using Svg.Skia;
using SkiaSharp.Views.Desktop;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Timers;

namespace PowerTool
{
    public partial class MainWindow : Window
    {
        private readonly SKSvg svgComputer;
        private readonly SKSvg svgScript;
        private readonly SKSvg svgRemote;
        private readonly SKSvg svgFolder;
        private readonly SKSvg svgPrograms;
        private readonly SKSvg svgServices;
        private readonly object equiposLock = new object();
        private ObservableCollection<Equipo> equipos;
        private System.Timers.Timer pingTimer;
        private System.Timers.Timer usuarioTimer;
        private DomainInfo selectedDomain;
        private int onlineCount = 0;
        private int offlineCount = 0;
        private int usersOnlineCount = 0;
        private int usersOfflineCount = 0;

        public MainWindow()
        {
            InitializeComponent();

            pingTimer = new System.Timers.Timer(5000);
            pingTimer.Elapsed += PingEquipos;
            pingTimer.Start();

            usuarioTimer = new System.Timers.Timer(5000); // Actualiza cada 60 segundos
            usuarioTimer.Elapsed += ActualizarUsuarios;
            usuarioTimer.Start();

             // Cargar los SVGs
            svgComputer = new SKSvg();
            svgComputer.Load(Path.Combine("Icons", "computer.svg"));

            svgScript = new SKSvg();
            svgScript.Load(Path.Combine("Icons", "script.svg"));

            svgRemote = new SKSvg();
            svgRemote.Load(Path.Combine("Icons", "remote.svg"));

            svgFolder = new SKSvg();
            svgFolder.Load(Path.Combine("Icons", "folder.svg"));

            svgPrograms = new SKSvg();
            svgPrograms.Load(Path.Combine("Icons", "programs.svg"));

            svgServices = new SKSvg();
            svgServices.Load(Path.Combine("Icons", "services.svg"));

            equipos = new ObservableCollection<Equipo>();
            EquiposListView.ItemsSource = equipos;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(EquiposListView.ItemsSource);
            view.Filter = EquipoFilter;

            // Mostrar la ventana de diálogo para introducir el dominio
            DomainWindow domainWindow = new DomainWindow();
            if (domainWindow.ShowDialog() == true)
            {
                selectedDomain = domainWindow.SelectedDomain;
                CargarEquiposDelDominio(selectedDomain);
            }
            else
            {
                MessageBox.Show("No se ha seleccionado un dominio. La aplicación se cerrará.");
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

        private void OnPaintSurfaceFolder(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svgFolder.Picture);
        }

        private void OnPaintSurfacePrograms(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svgPrograms.Picture);
        }

        private void OnPaintSurfaceServices(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svgServices.Picture);
        }

        private List<string> ObtenerControladoresDeDominio()
        {
            List<string> controladores = new List<string>();
            try
            {
                Domain domain = Domain.GetCurrentDomain();
                foreach (DomainController dc in domain.DomainControllers)
                {
                    controladores.Add(dc.Name);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error al obtener los controladores de dominio", ex);
            }
            return controladores;
        }

        private void PingEquipos(object sender, ElapsedEventArgs e)
        {
            int tempOnlineCount = 0;
            int tempOfflineCount = 0;

            lock (equiposLock)
            {
                foreach (var equipo in equipos)
                {
                    string nombreEquipo = equipo.Name;
                    bool estaEncendido = EstaEncendido(nombreEquipo);

                    Dispatcher.Invoke(() =>
                    {
                        equipo.IsOnline = estaEncendido ? Brushes.Green : Brushes.Red;
                        if (estaEncendido)
                            tempOnlineCount++;
                        else
                            tempOfflineCount++;
                    });
                }
            }

            onlineCount = tempOnlineCount;
            offlineCount = tempOfflineCount;

            Dispatcher.Invoke(ActualizarEstadisticas);
        }

        private void ActualizarUsuarios(object sender, ElapsedEventArgs e)
        {
            int tempUsersOnlineCount = 0;
            int tempUsersOfflineCount = 0;

            lock (equiposLock)
            {
                foreach (var equipo in equipos)
                {
                    string nombreEquipo = equipo.Name;
                    if (equipo.IsOnline == Brushes.Green)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            equipo.CurrentUser = ObtenerUsuarioActual(nombreEquipo, selectedDomain);
                            if (equipo.CurrentUser != "N/A")
                                tempUsersOnlineCount++;
                            else
                                tempUsersOfflineCount++;
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            equipo.CurrentUser = "N/A";
                            tempUsersOfflineCount++;
                        });
                    }
                }
            }

            usersOnlineCount = tempUsersOnlineCount;
            usersOfflineCount = tempUsersOfflineCount;

            Dispatcher.Invoke(ActualizarEstadisticas);
        }

        private void ActualizarEstadisticas()
        {
            OnlineCount.Text = onlineCount.ToString();
            OfflineCount.Text = offlineCount.ToString();
            UsersOnlineCount.Text = usersOnlineCount.ToString();
            UsersOfflineCount.Text = usersOfflineCount.ToString();
        }

        private bool EquipoFilter(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            return (item as Equipo).Name.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private async void CargarEquiposDelDominio(DomainInfo selectedDomain)
        {
            try
            {
                string password = EncryptionHelper.DecryptString(selectedDomain.EncryptedPassword);
                DirectoryEntry entry = new DirectoryEntry($"LDAP://{selectedDomain.DomainName}", selectedDomain.Username, password);
                DirectorySearcher searcher = new DirectorySearcher(entry)
                {
                    Filter = "(objectCategory=computer)"
                };

                searcher.PropertiesToLoad.Add("name");
                searcher.PropertiesToLoad.Add("description");
                searcher.PropertiesToLoad.Add("operatingSystem");
                searcher.PropertiesToLoad.Add("operatingSystemVersion");

                SearchResultCollection resultados = searcher.FindAll();
                List<Task<Equipo>> tareas = new List<Task<Equipo>>();

                List<string> controladoresDeDominio = ObtenerControladoresDeDominio();
                string nombreEquipoLocal = Environment.MachineName.Trim().ToLowerInvariant();

                foreach (SearchResult resultado in resultados)
                {
                    string nombre = resultado.Properties["name"].Count > 0 ? resultado.Properties["name"][0].ToString().Trim().ToLowerInvariant() : "";
                    if (!string.Equals(nombre, nombreEquipoLocal, StringComparison.Ordinal))
                    {
                        tareas.Add(Task.Run(async () =>
                        {
                            string? nombre = resultado.Properties["name"].Count > 0 ? resultado.Properties["name"][0].ToString() : "";
                            string? descripcion = resultado.Properties["description"].Count > 0 ? resultado.Properties["description"][0].ToString() : "";
                            string? sistemaOperativo = resultado.Properties["operatingSystem"].Count > 0 ? resultado.Properties["operatingSystem"][0].ToString() : "";
                            string? versionSO = resultado.Properties["operatingSystemVersion"].Count > 0 ? resultado.Properties["operatingSystemVersion"][0].ToString() : "";
                            DateTime lastLogon = await ObtenerUltimoInicioSesion(nombre, controladoresDeDominio);
                            var (ipAddress, macAddress) = ObtenerIPyMAC(nombre, selectedDomain);

                            var equipo = new Equipo
                            {
                                Name = nombre,
                                Description = descripcion,
                                OperatingSystem = sistemaOperativo,
                                OperatingSystemVersion = versionSO,
                                LastLogon = lastLogon,
                                IsOnline = Brushes.Red,
                                CurrentUser = ObtenerUsuarioActual(nombre, selectedDomain),
                                IPAddress = ipAddress,
                                MACAddress = macAddress
                            };

                            return equipo;
                        }));
                    }
                }

                Equipo[] equiposCargados = await Task.WhenAll(tareas);
                foreach (var equipo in equiposCargados)
                {
                    equipos.Add(equipo);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al cargar los equipos del dominio {selectedDomain.DomainName}", ex);
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

        private string ObtenerUsuarioActual(string nombreEquipo, DomainInfo selectedDomain)
        {
            try
            {
                ManagementScope scope;
                string password = EncryptionHelper.DecryptString(selectedDomain.EncryptedPassword);

                ConnectionOptions options = new ConnectionOptions();
                options.Username = selectedDomain.Username;
                options.Password = password;

                scope = new ManagementScope($@"\\{nombreEquipo}\root\cimv2", options);
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

        private (string ipAddress, string macAddress) ObtenerIPyMAC(string nombreEquipo, DomainInfo selectedDomain)
        {
            try
            {
                ManagementScope scope;
                string nombreEquipoLocal = Environment.MachineName;
                string password = EncryptionHelper.DecryptString(selectedDomain.EncryptedPassword);

                ConnectionOptions options = new ConnectionOptions();
                options.Username = selectedDomain.Username;
                options.Password = password;

                scope = new ManagementScope($@"\\{nombreEquipo}\root\cimv2", options);
                scope.Connect();

                ObjectQuery query = new ObjectQuery("SELECT IPAddress, MACAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

                foreach (ManagementObject result in searcher.Get())
                {
                    string[] ipAddresses = (string[])result["IPAddress"];
                    string macAddress = result["MACAddress"]?.ToString();
                    return (ipAddresses.FirstOrDefault(), macAddress);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al obtener la IP y MAC del equipo {nombreEquipo}", ex);
            }
            return ("N/A", "N/A");
        }

        private List<InstalledProgram> ObtenerProgramasInstalados(string nombreEquipo, DomainInfo selectedDomain)
        {
            List<InstalledProgram> programas = new List<InstalledProgram>();

            try
            {
                string password = EncryptionHelper.DecryptString(selectedDomain.EncryptedPassword);

                ConnectionOptions options = new ConnectionOptions();
                options.Username = selectedDomain.Username;
                options.Password = password;

                ManagementScope scope = new ManagementScope($@"\\{nombreEquipo}\root\default", options);
                scope.Connect();

                ManagementClass registry = new ManagementClass(scope, new ManagementPath("StdRegProv"), null);

                // Clave del registro que contiene la lista de programas instalados
                const uint HKEY_LOCAL_MACHINE = 0x80000002;
                string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

                // Obtener las subclaves (los identificadores de los programas instalados)
                ManagementBaseObject inParams = registry.GetMethodParameters("EnumKey");
                inParams["hDefKey"] = HKEY_LOCAL_MACHINE;
                inParams["sSubKeyName"] = registryKey;

                ManagementBaseObject outParams = registry.InvokeMethod("EnumKey", inParams, null);

                string[]? subKeyNames = outParams?["sNames"] as string[];

                if (subKeyNames != null)
                {
                    foreach (string subKeyName in subKeyNames)
                    {
                        // Obtener el valor "DisplayName" de cada subclave
                        ManagementBaseObject inParamsGetStringValue = registry.GetMethodParameters("GetStringValue");
                        inParamsGetStringValue["hDefKey"] = HKEY_LOCAL_MACHINE;
                        inParamsGetStringValue["sSubKeyName"] = $@"{registryKey}\{subKeyName}";
                        inParamsGetStringValue["sValueName"] = "DisplayName";

                        ManagementBaseObject outParamsGetStringValue = registry.InvokeMethod("GetStringValue", inParamsGetStringValue, null);

                        int returnCode = Convert.ToInt32(outParamsGetStringValue["ReturnValue"]);
                        if (returnCode == 0) // 0 significa éxito
                        {
                            string? displayName = outParamsGetStringValue["sValue"]?.ToString();
                            if (!string.IsNullOrEmpty(displayName))
                            {
                                programas.Add(new InstalledProgram { Name = displayName });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al obtener programas instalados de {nombreEquipo}", ex);
            }

            return programas;
        }

        private List<ServiceInfo> ObtenerServiciosEnEjecucion(string nombreEquipo, DomainInfo selectedDomain)
        {
            List<ServiceInfo> servicios = new List<ServiceInfo>();

            try
            {
                string password = EncryptionHelper.DecryptString(selectedDomain.EncryptedPassword);

                ConnectionOptions options = new ConnectionOptions();
                options.Username = selectedDomain.Username;
                options.Password = password;

                ManagementScope scope = new ManagementScope($@"\\{nombreEquipo}\root\cimv2", options);
                scope.Connect();

                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Service");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

                foreach (ManagementObject service in searcher.Get())
                {
                    servicios.Add(new ServiceInfo
                    {
                        Name = service["Name"]?.ToString(),
                        DisplayName = service["DisplayName"]?.ToString(),
                        State = service["State"]?.ToString(),
                        StartMode = service["StartMode"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al obtener servicios de {nombreEquipo}", ex);
            }

            return servicios;
        }

        private async Task<DateTime> ObtenerUltimoInicioSesion(string nombreEquipo, List<string> controladoresDeDominio)
        {
            DateTime ultimoInicioSesion = DateTime.MinValue;

            try
            {
                string filtro = $"(sAMAccountName={nombreEquipo}$)"; // El $ es importante para equipos
                string[] propiedades = { "lastLogon" };

                List<Task<DateTime>> tareas = new List<Task<DateTime>>();

                foreach (string dc in controladoresDeDominio)
                {
                    tareas.Add(Task.Run(() =>
                    {
                        try
                        {
                            string password = EncryptionHelper.DecryptString(selectedDomain.EncryptedPassword);
                            DirectoryEntry entry = new DirectoryEntry($"LDAP://{selectedDomain.DomainName}", selectedDomain.Username, password);
                            DirectorySearcher searcher = new DirectorySearcher(entry)
                            {
                                Filter = filtro
                            };
                            searcher.PropertiesToLoad.AddRange(propiedades);

                            SearchResult result = searcher.FindOne();
                            if (result != null && result.Properties.Contains("lastLogon"))
                            {
                                long lastLogonTicks = (long)result.Properties["lastLogon"][0];
                                if (lastLogonTicks > 0)
                                {
                                    DateTime lastLogon = DateTime.FromFileTime(lastLogonTicks);
                                    return lastLogon;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error al obtener lastLogon de {nombreEquipo} en controlador {dc}", ex);
                        }
                        return DateTime.MinValue;
                    }));
                }

                DateTime[] resultados = await Task.WhenAll(tareas);

                ultimoInicioSesion = resultados.Max();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al obtener el último inicio de sesión preciso para {nombreEquipo}", ex);
            }
            return ultimoInicioSesion;
        }

        private void AbrirExploradorArchivos(string nombreEquipo)
        {
            try
            {
                string ruta = $@"\\{nombreEquipo}\C$";
                System.Diagnostics.Process.Start("explorer.exe", ruta);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al intentar acceder al sistema de archivos de {nombreEquipo}", ex);
            }
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

        private void AbrirExploradorArchivos_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Equipo equipoSeleccionado)
            {
                AbrirExploradorArchivos(equipoSeleccionado.Name);
            }
        }

        private void ConectarRDPButton_Click(object sender, RoutedEventArgs e)
        {
            // Ahora requiere la selección explícita del equipo
            if (sender is Button button && button.DataContext is Equipo equipoSeleccionado)
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

        private void VerProgramasInstalados_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Equipo equipoSeleccionado)
            {
                List<InstalledProgram> programas = ObtenerProgramasInstalados(equipoSeleccionado.Name, selectedDomain);

                // Mostrar los programas en una nueva ventana
                ProgramListWindow programListWindow = new ProgramListWindow(programas, equipoSeleccionado.Name);
                programListWindow.Show();
            }
        }

        private void VerServiciosEnEjecucion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Equipo equipoSeleccionado)
            {
                List<ServiceInfo> servicios = ObtenerServiciosEnEjecucion(equipoSeleccionado.Name, selectedDomain);

                // Mostrar los servicios en una nueva ventana
                ServiceListWindow serviceListWindow = new ServiceListWindow(servicios, equipoSeleccionado.Name);
                serviceListWindow.Show();
            }
        }

        private void OpenUserManagementWindow_Click(object sender, RoutedEventArgs e)
        {
            UserManagementWindow userManagementWindow = new UserManagementWindow(selectedDomain);
            userManagementWindow.Show();
        }

        private void CerrarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}