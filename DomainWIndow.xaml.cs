using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.IO;
using System.Windows;
using PowerTool.Models;
using PowerTool.Utilities;

namespace PowerTool
{
    public partial class DomainWindow : Window
    {
        public ObservableCollection<DomainInfo> SavedDomains { get; set; }

        public DomainInfo SelectedDomain { get; private set; }

        public DomainWindow()
        {
            InitializeComponent();
            SavedDomains = LoadSavedDomains();
            this.DataContext = this;
        }

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

        private void AddDomain_Click(object sender, RoutedEventArgs e)
        {
            DomainEditWindow editWindow = new DomainEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                SavedDomains.Add(editWindow.DomainInfo);
                SaveDomains();
            }
        }

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

        private void DeleteDomain_Click(object sender, RoutedEventArgs e)
        {
            if (DomainsListView.SelectedItem is DomainInfo domainInfo)
            {
                SavedDomains.Remove(domainInfo);
                SaveDomains();
            }
        }

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
