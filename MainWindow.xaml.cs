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
using Microsoft.Win32;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

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
        private readonly SKSvg svgEventLog;
        private readonly SKSvg svgTask;
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
            try {
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

                svgEventLog = new SKSvg();
                svgEventLog.Load(Path.Combine("Icons", "services.svg"));

                svgTask = new SKSvg();
                svgTask.Load(Path.Combine("Icons", "services.svg"));
            }
            catch (Exception ex)
            {
                Logger.LogError("Error al cargar los SVG", ex);
            }
            

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

        private void OnPaintSurfaceTask(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svgTask.Picture);
        }

        private void OnPaintSurfaceEventLog(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svgEventLog.Picture);
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

        public ObservableCollection<RemoteProcess> ObtenerProcesosRemotos(string remoteMachineName)
        {
            ObservableCollection<RemoteProcess> procesos = new ObservableCollection<RemoteProcess>();
            try
            {
                string password = EncryptionHelper.DecryptString(selectedDomain.EncryptedPassword);
                var scope = CrearScope(remoteMachineName, password);

                if (!scope.IsConnected)
                {
                    Logger.LogError($"No se pudo conectar al equipo {remoteMachineName} para obtener los procesos.", null);
                    MessageBox.Show($"No se pudo conectar al equipo {remoteMachineName}. Consulte el log para más detalles.");
                    return procesos;
                }

                var query = new ObjectQuery("SELECT ProcessId, Name, WorkingSetSize FROM Win32_Process");
                var searcher = new ManagementObjectSearcher(scope, query);
                var processCollection = searcher.Get();

                if (processCollection.Count == 0)
                {
                    MessageBox.Show($"No se encontraron procesos en {remoteMachineName}. Esto podría deberse a permisos insuficientes o configuración de WMI.");
                    return procesos;
                }

                foreach (ManagementObject process in processCollection)
                {
                    var remoteProcess = new RemoteProcess
                    {
                        ProcessId = (int)(uint)process["ProcessId"],
                        Name = process["Name"]?.ToString(),
                        MemoryUsage = Convert.ToDouble(process["WorkingSetSize"]) / (1024 * 1024) // Convertir a MB
                    };
                    procesos.Add(remoteProcess);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al obtener procesos de {remoteMachineName}", ex);
                MessageBox.Show($"Error al obtener los procesos de {remoteMachineName}. Consulte el log para más detalles.");
            }
            return procesos;
        }

        public void TerminarProcesoRemoto(string remoteMachineName, int processId)
        {
            try
            {
                string password = EncryptionHelper.DecryptString(selectedDomain.EncryptedPassword);
                var scope = CrearScope(remoteMachineName, password);

                var process = new ManagementObject(scope, new ManagementPath($"Win32_Process.Handle='{processId}'"), null);
                process.InvokeMethod("Terminate", null);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al terminar el proceso {processId} en {remoteMachineName}", ex);
                MessageBox.Show("Error al terminar el proceso. Consulte el log para más detalles.");
            }
        }

        private ManagementScope CrearScope(string remoteMachineName, string decryptedPassword)
        {
            var options = new ConnectionOptions
            {
                Username = selectedDomain.Username,
                Password = decryptedPassword,
                Impersonation = ImpersonationLevel.Impersonate,
                Authentication = AuthenticationLevel.PacketPrivacy
            };

            var scope = new ManagementScope($@"\\{remoteMachineName}\root\cimv2", options);
            try
            {
                scope.Connect();
                if (!scope.IsConnected)
                {
                    Logger.LogError($"Error al conectar al equipo remoto: {remoteMachineName}", null);
                    MessageBox.Show($"Error al conectar al equipo {remoteMachineName}. Verifique las credenciales y permisos.");
                }
            }
            catch (UnauthorizedAccessException uae)
            {
                Logger.LogError($"Acceso denegado al conectar con {remoteMachineName}", uae);
                MessageBox.Show("Acceso denegado. Asegúrese de que las credenciales y permisos son correctos.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al conectar con {remoteMachineName}", ex);
                MessageBox.Show("No se pudo conectar al equipo remoto. Consulte el log para más detalles.");
            }

            return scope;
        }

        private List<RemoteEventLogEntry> ObtenerEventosRemotos(string nombreEquipo, DateTime startTime, DateTime endTime)
        {
            List<RemoteEventLogEntry> eventos = new List<RemoteEventLogEntry>();

            try
            {
                string password = EncryptionHelper.DecryptString(selectedDomain.EncryptedPassword);
                var scope = CrearScope(nombreEquipo, password);

                if (!scope.IsConnected)
                {
                    MessageBox.Show($"No se pudo conectar al equipo {nombreEquipo}. Consulte el log para más detalles.");
                    return eventos;
                }

                // Filtrar eventos de tipo Error (2) o Crítico (1)
                string queryStr = $"SELECT * FROM Win32_NTLogEvent WHERE Logfile = 'System' AND EventType <= 2 AND " +
                                $"TimeGenerated >= '{ManagementDateTimeConverter.ToDmtfDateTime(startTime)}' AND " +
                                $"TimeGenerated <= '{ManagementDateTimeConverter.ToDmtfDateTime(endTime)}'";

                ObjectQuery query = new ObjectQuery(queryStr);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

                foreach (ManagementObject evt in searcher.Get())
                {
                    var logEntry = new RemoteEventLogEntry
                    {
                        Source = evt["SourceName"]?.ToString(),
                        EventType = evt["EventType"]?.ToString(),
                        TimeGenerated = ManagementDateTimeConverter.ToDateTime(evt["TimeGenerated"].ToString()),
                        Message = evt["Message"]?.ToString()
                    };
                    eventos.Add(logEntry);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error al obtener eventos remotos de {nombreEquipo}", ex);
            }

            return eventos;
        }

        private void TransferirEInstalarArchivo(Equipo equipoSeleccionado)
        {
            // Seleccionar archivo local
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Selecciona el archivo de instalación",
                Filter = "Archivos ejecutables (*.exe)|*.exe|Todos los archivos (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true)
            {
                MessageBox.Show("No se seleccionó ningún archivo.");
                return;
            }

            string localPath = openFileDialog.FileName;

            // Definir ruta remota fija
            string remoteDirectory = @"C:\temp";
            string remotePath = $"{remoteDirectory}\\{Path.GetFileName(localPath)}";

            try
            {
                string remoteMachine = equipoSeleccionado.Name;
                string username = selectedDomain.Username;
                string password = EncryptionHelper.DecryptString(selectedDomain.EncryptedPassword);

                using (PowerShell ps = PowerShell.Create())
                {
                    // Crear la sesión remota
                    ps.AddScript($@"
                        $securePassword = ConvertTo-SecureString '{password}' -AsPlainText -Force
                        $credential = New-Object System.Management.Automation.PSCredential ('{username}', $securePassword)
                        $session = New-PSSession -ComputerName '{remoteMachine}' -Credential $credential
                        $session
                    ");

                    var results = ps.Invoke();
                    if (ps.HadErrors || results == null || results.Count == 0)
                    {
                        string errorMessages = "Error al crear la sesión remota:\n";
                        foreach (var error in ps.Streams.Error)
                        {
                            errorMessages += $"{error.Exception.Message}\n";
                        }

                        MessageBox.Show(errorMessages);
                        return;
                    }

                    var session = results[0];

                    // Asegurarse de que el directorio remoto existe
                    ps.Commands.Clear();
                    ps.AddScript($"Invoke-Command -Session $session -ScriptBlock {{ if (!(Test-Path -Path '{remoteDirectory}')) {{ New-Item -ItemType Directory -Path '{remoteDirectory}' }} }}");
                    ps.Invoke();

                    if (ps.HadErrors)
                    {
                        string errorMessages = "Error al crear el directorio remoto:\n";
                        foreach (var error in ps.Streams.Error)
                        {
                            errorMessages += $"{error.Exception.Message}\n";
                        }

                        MessageBox.Show(errorMessages);
                        return;
                    }

                    // Transferir el archivo
                    ps.Commands.Clear();
                    ps.AddScript($"Copy-Item -Path '{localPath}' -Destination '{remotePath}' -ToSession $session");
                    ps.Invoke();

                    if (ps.HadErrors)
                    {
                        string errorMessages = "Error al transferir archivo:\n";
                        foreach (var error in ps.Streams.Error)
                        {
                            errorMessages += $"{error.Exception.Message}\n";
                        }

                        MessageBox.Show(errorMessages);
                        return;
                    }

                    MessageBox.Show("Archivo transferido exitosamente a C:\\temp. Iniciando instalación...");

                    // Ejecutar el instalador como trabajo en segundo plano
                    ps.Commands.Clear();
                    ps.AddScript($"Invoke-Command -Session $session -ScriptBlock {{ Start-Process '{remotePath}' -ArgumentList '/S /V/qn' -Verb RunAs -Wait }} -AsJob");
                    ps.Invoke();

                    if (ps.HadErrors)
                    {
                        string errorMessages = "Error durante la instalación en el equipo remoto:\n";
                        foreach (var error in ps.Streams.Error)
                        {
                            errorMessages += $"{error.Exception.Message}\n";
                        }

                        MessageBox.Show(errorMessages);
                    }
                    else
                    {
                        MessageBox.Show("Instalación completada exitosamente en el equipo remoto.");
                    }

                    // Cerrar la sesión remota
                    ps.Commands.Clear();
                    ps.AddScript("Remove-PSSession -Session $session");
                    ps.Invoke();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error al transferir e instalar archivo en el equipo remoto.", ex);
                MessageBox.Show("Error en la transferencia o instalación. Verifica el log para más detalles.");
            }
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

        public async Task<ObservableCollection<HardwareInfo>> ObtenerInventarioHardwareAsync(List<Equipo> equipos)
        {
            var inventario = new ObservableCollection<HardwareInfo>();

            await Task.Run(() =>
            {
                foreach (var equipo in equipos)
                {
                    try
                    {
                        var scope = CrearScope(equipo.Name, EncryptionHelper.DecryptString(selectedDomain.EncryptedPassword));

                        // Información de CPU
                        var cpuQuery = new ObjectQuery("SELECT Name, NumberOfCores, MaxClockSpeed FROM Win32_Processor");
                        var cpuSearcher = new ManagementObjectSearcher(scope, cpuQuery);
                        string cpuNombre = "";
                        string cpuDetalles = "";

                        foreach (ManagementObject cpu in cpuSearcher.Get())
                        {
                            cpuNombre = cpu["Name"]?.ToString();
                            cpuDetalles = $"Cores: {cpu["NumberOfCores"]}, Max Speed: {cpu["MaxClockSpeed"]} MHz";
                        }

                        // Información de RAM
                        var ramQuery = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
                        var ramSearcher = new ManagementObjectSearcher(scope, ramQuery);
                        double totalRam = 0;
                        foreach (ManagementObject ram in ramSearcher.Get())
                        {
                            totalRam += Convert.ToDouble(ram["Capacity"]) / (1024 * 1024 * 1024); // Sumar la RAM de cada módulo
                        }
                        totalRam = Math.Round(totalRam, 2);

                        // Información de Disco
                        var diskQuery = new ObjectQuery("SELECT DeviceID, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3");
                        var diskSearcher = new ManagementObjectSearcher(scope, diskQuery);
                        double totalDiskSpace = 0;
                        double freeDiskSpace = 0;

                        foreach (ManagementObject disk in diskSearcher.Get())
                        {
                            totalDiskSpace += Math.Round(Convert.ToDouble(disk["Size"]) / (1024 * 1024 * 1024), 2); // GB
                            freeDiskSpace += Math.Round(Convert.ToDouble(disk["FreeSpace"]) / (1024 * 1024 * 1024), 2); // GB
                        }

                        // Crear el objeto de información de hardware
                        var hardwareInfo = new HardwareInfo
                        {
                            MachineName = equipo.Name,
                            Cpu = cpuNombre,
                            CpuDetails = cpuDetalles,
                            RamInGB = totalRam,
                            DiskSpaceInGB = totalDiskSpace,
                            DiskFreeSpaceInGB = freeDiskSpace
                        };

                        // Añadir al inventario en el hilo de la interfaz de usuario
                        App.Current.Dispatcher.Invoke(() => inventario.Add(hardwareInfo));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error al obtener inventario de hardware para {equipo.Name}", ex);
                    }
                }
            });

            return inventario;
        }

        private void EquiposListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (EquiposListView.SelectedItem is Equipo equipoSeleccionado)
            {
                // Configurar el DataContext del ContextMenu con el equipo seleccionado
                EquiposListView.ContextMenu.DataContext = equipoSeleccionado;
            }
            else
            {
                // Si no hay un equipo seleccionado, cancelar el menú contextual
                e.Handled = true;
                MessageBox.Show("Por favor, seleccione un equipo de la lista.");
            }
        }

        private void AbrirExploradorArchivos_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is Equipo equipoSeleccionado)
            {
                try
                {
                    string ruta = $@"\\{equipoSeleccionado.Name}\C$";
                    System.Diagnostics.Process.Start("explorer.exe", ruta);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error al intentar acceder al sistema de archivos de {equipoSeleccionado.Name}", ex);
                    MessageBox.Show("Error al intentar abrir el explorador de archivos. Consulte el log para más detalles.");
                }
            }
        }

        private void AbrirPopUpComando_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is Equipo equipoSeleccionado)
            {
                CommandWindow commandWindow = new CommandWindow(equipoSeleccionado);
                commandWindow.ShowDialog();
            }
        }

        private void ConectarRDPButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is Equipo equipoSeleccionado)
            {
                try
                {
                    System.Diagnostics.Process.Start("mstsc", $"/v:{equipoSeleccionado.Name}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error al intentar conectarse al equipo {equipoSeleccionado.Name} mediante RDP", ex);
                    MessageBox.Show($"Error al intentar conectarse al equipo {equipoSeleccionado.Name} mediante RDP: {ex.Message}");
                }
            }
        }

        private void VerProgramasInstalados_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is Equipo equipoSeleccionado)
            {
                List<InstalledProgram> programas = ObtenerProgramasInstalados(equipoSeleccionado.Name, selectedDomain);
                ProgramListWindow programListWindow = new ProgramListWindow(programas, equipoSeleccionado.Name);
                programListWindow.Show();
            }
        }

        private void VerServiciosEnEjecucion_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is Equipo equipoSeleccionado)
            {
                List<ServiceInfo> servicios = ObtenerServiciosEnEjecucion(equipoSeleccionado.Name, selectedDomain);
                ServiceListWindow serviceListWindow = new ServiceListWindow(servicios, equipoSeleccionado.Name, selectedDomain);
                serviceListWindow.Show();
            }
        }

        private void RemoteTaskManager_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is Equipo equipoSeleccionado)
            {
                var taskManagerWindow = new RemoteTaskManagerWindow(equipoSeleccionado.Name, this);
                taskManagerWindow.Show();
            }
        }

        private void OpenRemoteEventLog_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is Equipo equipoSeleccionado)
            {
                var eventos = ObtenerEventosRemotos(equipoSeleccionado.Name, DateTime.Now.AddDays(-7), DateTime.Now);
                var observableEventos = new ObservableCollection<RemoteEventLogEntry>(eventos);
                var eventLogWindow = new RemoteEventLogWindow(observableEventos);
                eventLogWindow.Show();
            }
        }

        private void TransferirEInstalarArchivo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is Equipo equipoSeleccionado)
            {
                TransferirEInstalarArchivo(equipoSeleccionado);
            }
        }

        private void OpenUserManagementWindow_Click(object sender, RoutedEventArgs e)
        {
            UserManagementWindow userManagementWindow = new UserManagementWindow(selectedDomain);
            userManagementWindow.Show();
        }

        private async void VerInventarioHardware_Click(object sender, RoutedEventArgs e)
        {
            var hardwareInfos = await ObtenerInventarioHardwareAsync(equipos.ToList());
            var hardwareInventoryWindow = new HardwareInventoryWindow(hardwareInfos);
            hardwareInventoryWindow.Show();
        }

        private void CerrarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}