using System.Collections.ObjectModel;
using System.Windows;
using PowerTool.Models;

namespace PowerTool.Views
{
    public partial class HardwareInventoryWindow : Window
    {
        public ObservableCollection<HardwareInfo> HardwareInfos { get; set; }

        public HardwareInventoryWindow(ObservableCollection<HardwareInfo> hardwareInfos)
        {
            InitializeComponent();
            HardwareInfos = hardwareInfos;
            HardwareListView.ItemsSource = HardwareInfos;
        }
    }
}
