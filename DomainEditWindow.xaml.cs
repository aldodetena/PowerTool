using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PowerTool.Models;
using PowerTool.Services;

namespace PowerTool
{
    public partial class DomainEditWindow : Window
    {
        public DomainInfo DomainInfo { get; private set; }

        public DomainEditWindow()
        {
            InitializeComponent();
            DomainInfo = new DomainInfo();
            this.DataContext = DomainInfo;
        }

        public DomainEditWindow(DomainInfo domainInfo)
        {
            InitializeComponent();
            DomainInfo = domainInfo;
            this.DataContext = DomainInfo;
            passwordBox.Password = EncryptionHelper.DecryptString(domainInfo.EncryptedPassword);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DomainInfo.EncryptedPassword = EncryptionHelper.EncryptString(passwordBox.Password);
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}