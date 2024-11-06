using System.Windows;
using PowerTool.Models;
using PowerTool.Services;

namespace PowerTool.Views
{
    /// <summary>
    /// Representa una ventana para ejecutar comandos remotos en un equipo.
    /// </summary>
    public partial class CommandWindow : Window
    {
        private Equipo equipo;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="CommandWindow"/> con el equipo seleccionado.
        /// </summary>
        /// <param name="equipoSeleccionado">El equipo en el que se ejecutará el comando.</param>
        public CommandWindow(Equipo equipoSeleccionado)
        {
            InitializeComponent();
            equipo = equipoSeleccionado;
        }

        /// <summary>
        /// Maneja el evento de clic para ejecutar un comando remoto en el equipo seleccionado.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void EjecutarComando_Click(object sender, RoutedEventArgs e)
        {
            string comando = ComandoTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(comando))
            {
                string resultado = RemoteCommandService.EjecutarComandoRemoto(equipo, comando);
                MessageBox.Show($"Resultado del comando en {equipo.Name}:\n{resultado}");
                this.Close();
            }
            else
            {
                MessageBox.Show("Por favor, introduce un comando.");
            }
        }
    }
}
