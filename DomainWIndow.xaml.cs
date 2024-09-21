using System.Windows;

namespace PowerTool
{
    public partial class DomainWindow : Window
    {
        public string DomainName { get; private set; }

        public DomainWindow()
        {
            InitializeComponent();
        }

        private void ConectarButton_Click(object sender, RoutedEventArgs e)
        {
            // Captura el dominio introducido y lo almacena
            DomainName = DomainTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(DomainName))
            {
                this.DialogResult = true; // Indica que se ha completado correctamente
                this.Close();
            }
            else
            {
                MessageBox.Show("Por favor, introduce un dominio válido.");
            }
        }
    }
}
