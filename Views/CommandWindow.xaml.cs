using System.Windows;
using PowerTool.Models;
using PowerTool.Services;

namespace PowerTool.Views
{
    public partial class CommandWindow : Window
    {
        private Equipo equipo;

        public CommandWindow(Equipo equipoSeleccionado)
        {
            InitializeComponent();
            equipo = equipoSeleccionado;
        }

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
