using System.Collections.ObjectModel;
using System.Windows;
using PowerTool.Models;

namespace PowerTool.Views
{
    public partial class RemoteTaskManagerWindow : Window
    {
        private string _remoteMachineName;
        public ObservableCollection<RemoteProcess> Processes { get; set; }
        private readonly MainWindow _mainWindow;

        public RemoteTaskManagerWindow(string remoteMachineName, MainWindow mainWindow)
        {
            InitializeComponent();
            _remoteMachineName = remoteMachineName;
            _mainWindow = mainWindow;
            Processes = new ObservableCollection<RemoteProcess>();
            DataContext = this;
            LoadProcesses();
        }

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

        private void RefreshProcessList_Click(object sender, RoutedEventArgs e)
        {
            LoadProcesses();
        }

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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
