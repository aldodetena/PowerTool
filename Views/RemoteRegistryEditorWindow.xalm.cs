using System.Collections.ObjectModel;
using System.Windows;
using PowerTool.Models;
using PowerTool.Utilities;

namespace PowerTool.Views
{
    public partial class RemoteRegistryEditorWindow : Window
    {
        private readonly string _remoteMachineName;
        private readonly string registryKey;
        private readonly DomainInfo _selectedDomain;
        private readonly MainWindow _mainWindow;

        public ObservableCollection<RegistryEntry> RegistryEntries { get; set; }

        public RemoteRegistryEditorWindow(string remoteMachineName, DomainInfo domainInfo, MainWindow mainWindow)
        {
            InitializeComponent();
            _remoteMachineName = remoteMachineName;
            _selectedDomain = domainInfo;
            _mainWindow = mainWindow;

            var registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion";
            var clavesRegistro = _mainWindow.ObtenerClavesRegistroRemoto(_remoteMachineName, registryKey, _selectedDomain);
            RegistryEntries = new ObservableCollection<RegistryEntry>(clavesRegistro);
            Logger.LogError($"Cargadas {RegistryEntries.Count} claves de registro desde {_remoteMachineName}.", null);

            DataContext = this;
        }

        private void LoadRegistryEntries()
        {
            RegistryEntries.Clear();
            var entries = _mainWindow.ObtenerClavesRegistroRemoto(_remoteMachineName, registryKey, _selectedDomain);

            foreach (var entry in entries)
            {
                RegistryEntries.Add(entry);
            }
        }

        private void EditRegistryEntry_Click(object sender, RoutedEventArgs e)
        {
            if (RegistryListView.SelectedItem is RegistryEntry selectedEntry)
            {
                var newValue = PromptEditRegistryEntry(selectedEntry);
                if (newValue != null)
                {
                    bool success = _mainWindow.EditarValorRegistroRemoto(_remoteMachineName, selectedEntry.Key, selectedEntry.Value, newValue);
                    if (success)
                    {
                        selectedEntry.Value = newValue;
                        MessageBox.Show("Valor del registro editado correctamente.");
                        Logger.LogError($"Valor del registro '{selectedEntry.Key}' editado en {_remoteMachineName}.", null);
                    }
                    else
                    {
                        MessageBox.Show("No se pudo editar el valor del registro. Consulte el log para más detalles.");
                    }
                }
            }
        }

        private string PromptEditRegistryEntry(RegistryEntry entry)
        {
            // Aquí implementarías un pop-up para permitir al usuario editar el valor
            return entry.Value; // Placeholder
        }

        private void DeleteRegistryEntry_Click(object sender, RoutedEventArgs e)
        {
            if (RegistryListView.SelectedItem is RegistryEntry selectedEntry)
            {
                bool success = _mainWindow.EliminarClaveRegistroRemoto(_remoteMachineName, selectedEntry.Key);
                if (success)
                {
                    RegistryEntries.Remove(selectedEntry);
                    MessageBox.Show("Clave de registro eliminada correctamente.");
                    Logger.LogError($"Clave de registro '{selectedEntry.Key}' eliminada en {_remoteMachineName}.", null);
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar la clave de registro. Consulte el log para más detalles.");
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
