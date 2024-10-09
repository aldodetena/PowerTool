using System.Windows;

namespace PowerTool.Views
{
    public partial class PasswordInputDialog : Window
    {
        public string Password { get; private set; }

        public PasswordInputDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Password = PasswordBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
