using System.Windows;
using PowerTool.Models;

namespace PowerTool.Views
{
    /// <summary>
    /// Representa una ventana para mostrar la lista de programas instalados en un equipo remoto.
    /// </summary>
    public partial class ProgramListWindow : Window
    {
        /// <summary>
        /// Inicializa una nueva instancia de <see cref="ProgramListWindow"/> con la lista de programas instalados y el nombre del equipo.
        /// </summary>
        /// <param name="programas">Lista de programas instalados en el equipo.</param>
        /// <param name="nombreEquipo">Nombre del equipo remoto.</param>
        public ProgramListWindow(List<InstalledProgram> programas, string nombreEquipo)
        {
            InitializeComponent();
            this.Title = $"Programas instalados en {nombreEquipo}";
            this.DataContext = programas;
        }
    }
}