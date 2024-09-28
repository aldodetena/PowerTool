using System.Windows;
using PowerTool.Services;
using PowerTool.Models;

namespace PowerTool
{
    public partial class ServiceListWindow : Window
    {
        public ServiceListWindow(List<ServiceInfo> servicios, string nombreEquipo)
        {
            InitializeComponent();
            this.Title = $"Servicios en ejecución en {nombreEquipo}";
            this.DataContext = servicios;
        }
    }
}