using System.Windows;

namespace PowerTool.Views
{
    /// <summary>
    /// Representa un cuadro de diálogo para ingresar una contraseña.
    /// </summary>
    public partial class PasswordInputDialog : Window
    {
        /// <summary>
        /// Obtiene la contraseña ingresada por el usuario.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="PasswordInputDialog"/>.
        /// </summary>
        public PasswordInputDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Maneja el evento Click del botón OK. Establece la contraseña y cierra el diálogo con resultado verdadero.
        /// </summary>
        /// <param name="sender">El botón OK que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Password = PasswordBox.Text;
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Maneja el evento Click del botón Cancelar. Cierra el diálogo con resultado falso.
        /// </summary>
        /// <param name="sender">El botón Cancelar que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
