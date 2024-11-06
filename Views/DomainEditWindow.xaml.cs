using System.Windows;
using PowerTool.Models;
using PowerTool.Services;

namespace PowerTool.Views
{
    /// <summary>
    /// Representa una ventana para editar la información de un dominio.
    /// </summary>
    public partial class DomainEditWindow : Window
    {
        /// <summary>
        /// Contiene la información del dominio que se está editando.
        /// </summary>
        public DomainInfo DomainInfo { get; private set; }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="DomainEditWindow"/> para un nuevo dominio.
        /// </summary>
        public DomainEditWindow()
        {
            InitializeComponent();
            DomainInfo = new DomainInfo();
            this.DataContext = DomainInfo;
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="DomainEditWindow"/> para editar un dominio existente.
        /// </summary>
        /// <param name="domainInfo">La información del dominio que se está editando.</param>
        public DomainEditWindow(DomainInfo domainInfo)
        {
            InitializeComponent();
            DomainInfo = domainInfo;
            this.DataContext = DomainInfo;
            passwordBox.Password = EncryptionHelper.DecryptString(domainInfo.EncryptedPassword);
        }

        /// <summary>
        /// Maneja el evento de clic para guardar la información del dominio editado.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DomainInfo.EncryptedPassword = EncryptionHelper.EncryptString(passwordBox.Password);
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Maneja el evento de clic para cancelar la edición y cerrar la ventana.
        /// </summary>
        /// <param name="sender">El botón que activó el evento.</param>
        /// <param name="e">Datos del evento RoutedEventArgs.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}