using System.Collections.ObjectModel;
using System.Windows;
using PowerTool.Models;

namespace PowerTool.Views
{
    /// <summary>
    /// Representa una ventana para la gestión de procesos remotos en un equipo específico.
    /// </summary>
    public partial class RemoteTaskManagerWindow : Window
    {   
        /// <summary>
        /// Nombre del equipo remoto del que se están gestionando los procesos.
        /// </summary>
        private string _remoteMachineName;
        /// <summary>
        /// Colección de procesos remotos actualmente cargados desde el equipo remoto.
        /// </summary>
        public ObservableCollection<RemoteProcess> Processes { get; set; }
        /// <summary>
        /// Referencia a la ventana principal para interactuar con las funcionalidades compartidas.
        /// </summary>
        private readonly MainWindow _mainWindow;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="RemoteTaskManagerWindow"/> para un equipo remoto específico.
        /// </summary>
        /// <param name="remoteMachineName">Nombre del equipo remoto.</param>
        /// <param name="mainWindow">Instancia de la ventana principal para acceso compartido.</param>
        public RemoteTaskManagerWindow(string remoteMachineName, MainWindow mainWindow)
        {
            InitializeComponent();
            _remoteMachineName = remoteMachineName;
            _mainWindow = mainWindow;
            Processes = new ObservableCollection<RemoteProcess>();
            DataContext = this;
            LoadProcesses();
        }

        /// <summary>
        /// Carga la lista de procesos en ejecución en el equipo remoto.
        /// </summary>
        private void LoadProcesses()
        {
            Processes.Clear();
            foreach (var process in _mainWindow.ObtenerProcesosRemotos(_remoteMachineName))
            {
                Processes.Add(process);
            }

            // Forzar la actualización de la interfaz
            ProcessListView.ItemsSource = null;
            ProcessListView.ItemsSource = Processes;
        }

        /// <summary>
        /// Maneja el evento de clic para refrescar la lista de procesos remotos.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void RefreshProcessList_Click(object sender, RoutedEventArgs e)
        {
            LoadProcesses();
        }

        /// <summary>
        /// Maneja el evento de clic para finalizar un proceso seleccionado en el equipo remoto.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void EndProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessListView.SelectedItem is RemoteProcess selectedProcess)
            {
                _mainWindow.TerminarProcesoRemoto(_remoteMachineName, selectedProcess.ProcessId);
                MessageBox.Show($"Proceso {selectedProcess.Name} terminado.");
                LoadProcesses(); // Refresca la lista de procesos después de terminar el proceso seleccionado
            }
            else
            {
                MessageBox.Show("Seleccione un proceso para terminar.");
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón de cierre, cerrando la ventana.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
