using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.IO;
using System.Windows;
using PowerTool.Models;
using PowerTool.Utilities;

namespace PowerTool.Views
{
    /// <summary>
    /// Representa una ventana para gestionar los dominios guardados y seleccionar uno.
    /// </summary>
    public partial class DomainWindow : Window
    {
        /// <summary>
        /// Colección de dominios guardados.
        /// </summary>
        public ObservableCollection<DomainInfo> SavedDomains { get; set; }

        /// <summary>
        /// El dominio seleccionado por el usuario.
        /// </summary>
        public DomainInfo SelectedDomain { get; private set; }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="DomainWindow"/> y carga los dominios guardados.
        /// </summary>
        public DomainWindow()
        {
            InitializeComponent();
            SavedDomains = LoadSavedDomains();
            this.DataContext = this;
        }

        /// <summary>
        /// Carga los dominios guardados desde un archivo XML.
        /// </summary>
        /// <returns>Una colección de dominios cargados.</returns>
        private ObservableCollection<DomainInfo> LoadSavedDomains()
        {
            try
            {
                if (File.Exists("domains.xml"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<DomainInfo>));
                    using (FileStream fs = new FileStream("domains.xml", FileMode.Open))
                    {
                        List<DomainInfo> domains = (List<DomainInfo>)serializer.Deserialize(fs);
                        return new ObservableCollection<DomainInfo>(domains);
                    }
                }
                else
                {
                    return new ObservableCollection<DomainInfo>();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error al cargar los dominios", ex);
                return new ObservableCollection<DomainInfo>();
            }
        }

        /// <summary>
        /// Guarda la lista de dominios en un archivo XML.
        /// </summary>
        private void SaveDomains()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<DomainInfo>));
                using (FileStream fs = new FileStream("domains.xml", FileMode.Create))
                {
                    serializer.Serialize(fs, SavedDomains.ToList());
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error al guardar los dominios", ex);
            }
        }

         /// <summary>
        /// Maneja el evento de clic para añadir un nuevo dominio.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void AddDomain_Click(object sender, RoutedEventArgs e)
        {
            DomainEditWindow editWindow = new DomainEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                SavedDomains.Add(editWindow.DomainInfo);
                SaveDomains();
            }
        }

        /// <summary>
        /// Maneja el evento de clic para editar un dominio seleccionado.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void EditDomain_Click(object sender, RoutedEventArgs e)
        {
            if (DomainsListView.SelectedItem is DomainInfo domainInfo)
            {
                DomainEditWindow editWindow = new DomainEditWindow(domainInfo);
                if (editWindow.ShowDialog() == true)
                {
                    // Actualizar la información del dominio
                    SaveDomains();
                }
            }
        }

        /// <summary>
        /// Maneja el evento de clic para eliminar un dominio seleccionado.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void DeleteDomain_Click(object sender, RoutedEventArgs e)
        {
            if (DomainsListView.SelectedItem is DomainInfo domainInfo)
            {
                SavedDomains.Remove(domainInfo);
                SaveDomains();
            }
        }

        /// <summary>
        /// Maneja el evento de clic para seleccionar un dominio de la lista.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void SelectDomain_Click(object sender, RoutedEventArgs e)
        {
            if (DomainsListView.SelectedItem is DomainInfo domainInfo)
            {
                SelectedDomain = domainInfo;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un dominio de la lista.");
            }
        }
    }
}
