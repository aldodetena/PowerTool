using System.Collections.ObjectModel;
using System.Windows;
using PowerTool.Models;

namespace PowerTool.Views
{
    /// <summary>
    /// Representa una ventana para mostrar el inventario de hardware de los equipos.
    /// </summary>
    public partial class HardwareInventoryWindow : Window
    {
        /// <summary>
        /// Colección de información de hardware para los equipos.
        /// </summary>
        public ObservableCollection<HardwareInfo> HardwareInfos { get; set; }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="HardwareInventoryWindow"/> con los datos de inventario proporcionados.
        /// </summary>
        /// <param name="hardwareInfos">La colección de datos de hardware para mostrar.</param>
        public HardwareInventoryWindow(ObservableCollection<HardwareInfo> hardwareInfos)
        {
            InitializeComponent();
            HardwareInfos = hardwareInfos;
            HardwareListView.ItemsSource = HardwareInfos;
        }
    }
}
