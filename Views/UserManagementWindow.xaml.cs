using System.Collections.ObjectModel;
using System.Windows;
using System.DirectoryServices.AccountManagement;
using PowerTool.Models;
using PowerTool.Utilities;
using PowerTool.Services;

namespace PowerTool.Views
{
    public partial class UserManagementWindow : Window
    {
        private DomainInfo _selectedDomain;
        public ObservableCollection<UserAccount> UserAccounts { get; set; }

        public UserManagementWindow(DomainInfo selectedDomain)
        {
            InitializeComponent();
            _selectedDomain = selectedDomain;
            UserAccounts = new ObservableCollection<UserAccount>();
            DataContext = this;
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                Logger.LogError("Inicio de carga de usuarios", null);

                string decryptedPassword = EncryptionHelper.DecryptString(_selectedDomain.EncryptedPassword);

                using (PrincipalContext context = new PrincipalContext(ContextType.Domain, _selectedDomain.DomainName, _selectedDomain.Username, decryptedPassword))
                {
                    UserPrincipal userPrincipal = new UserPrincipal(context);
                    using (PrincipalSearcher searcher = new PrincipalSearcher(userPrincipal))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            if (result is UserPrincipal user)
                            {
                                UserAccounts.Add(new UserAccount
                                {
                                    Name = user.SamAccountName,
                                    Status = user.Enabled.HasValue && user.Enabled.Value ? "Activo" : "Inactivo",
                                    LastLogin = user.LastLogon.HasValue ? user.LastLogon.Value.ToString("g") : "N/A",
                                    IsEnabled = user.Enabled.HasValue && user.Enabled.Value,
                                    IsLocked = user.IsAccountLockedOut()
                                });
                            }
                        }
                    }
                }

                UserListView.ItemsSource = UserAccounts;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error al cargar usuarios del dominio", ex);
                MessageBox.Show("Error al cargar usuarios. Consulte el log para más detalles.");
            }
        }

        private void ToggleEnable_Click(object sender, RoutedEventArgs e)
        {
            if (UserListView.SelectedItem is UserAccount selectedUser)
            {
                try
                {
                    string decryptedPassword = EncryptionHelper.DecryptString(_selectedDomain.EncryptedPassword);

                    using (var context = new PrincipalContext(ContextType.Domain, _selectedDomain.DomainName, _selectedDomain.Username, decryptedPassword))
                    using (var user = UserPrincipal.FindByIdentity(context, selectedUser.Name))
                    {
                        if (user != null)
                        {
                            user.Enabled = !user.Enabled;
                            user.Save();

                            // Actualizar las propiedades en el objeto seleccionado
                            selectedUser.IsEnabled = user.Enabled.HasValue && user.Enabled.Value;
                            selectedUser.Status = selectedUser.IsEnabled ? "Activo" : "Inactivo";

                            Logger.LogError($"Estado de activación de {selectedUser.Name} actualizado a {selectedUser.Status}.", null);
                            MessageBox.Show($"Estado de {selectedUser.Name} actualizado a {selectedUser.Status}.");
                        }
                        else
                        {
                            Logger.LogError($"Usuario {selectedUser.Name} no encontrado en el dominio", null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error al actualizar el estado de activación del usuario", ex);
                    MessageBox.Show("Error al actualizar el estado del usuario. Consulte el log para más detalles.");
                }
            }
        }

        private void UnlockUser_Click(object sender, RoutedEventArgs e)
        {
            if (UserListView.SelectedItem is UserAccount selectedUser)
            {
                try
                {
                    string decryptedPassword = EncryptionHelper.DecryptString(_selectedDomain.EncryptedPassword);

                    using (var context = new PrincipalContext(ContextType.Domain, _selectedDomain.DomainName, _selectedDomain.Username, decryptedPassword))
                    using (var user = UserPrincipal.FindByIdentity(context, selectedUser.Name))
                    {
                        if (user != null && user.IsAccountLockedOut())
                        {
                            user.UnlockAccount();

                            // Actualizar las propiedades en el objeto seleccionado
                            selectedUser.IsLocked = false;
                            selectedUser.Status = selectedUser.IsEnabled ? "Activo" : "Inactivo";

                            Logger.LogError($"Usuario {selectedUser.Name} desbloqueado.", null);
                            MessageBox.Show($"Usuario {selectedUser.Name} desbloqueado.");
                        }
                        else if (user != null)
                        {
                            MessageBox.Show($"El usuario {selectedUser.Name} no está bloqueado.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error al intentar desbloquear el usuario", ex);
                    MessageBox.Show("Error al intentar desbloquear el usuario. Consulte el log para más detalles.");
                }
            }
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (UserListView.SelectedItem is UserAccount selectedUser)
            {
                var passwordDialog = new PasswordInputDialog();
                if (passwordDialog.ShowDialog() == true)
                {
                    string newPassword = passwordDialog.Password;
                    if (string.IsNullOrEmpty(newPassword))
                    {
                        MessageBox.Show("La contraseña no puede estar vacía.");
                        return;
                    }

                    try
                    {
                        string decryptedPassword = EncryptionHelper.DecryptString(_selectedDomain.EncryptedPassword);

                        using (var context = new PrincipalContext(ContextType.Domain, _selectedDomain.DomainName, _selectedDomain.Username, decryptedPassword))
                        using (var user = UserPrincipal.FindByIdentity(context, selectedUser.Name))
                        {
                            if (user != null)
                            {
                                user.SetPassword(newPassword);
                                MessageBox.Show($"Contraseña de {selectedUser.Name} restablecida correctamente.");
                                Logger.LogError($"Contraseña de {selectedUser.Name} restablecida.", null);
                            }
                            else
                            {
                                Logger.LogError($"Usuario {selectedUser.Name} no encontrado en el dominio", null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Error al restablecer la contraseña", ex);
                        MessageBox.Show("Error al restablecer la contraseña. Consulte el log para más detalles.");
                    }
                }
            }
        }

        private void UserSearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserSearchBox.Text))
            {
                UserListView.ItemsSource = UserAccounts;
            }
            else
            {
                var filteredUsers = UserAccounts.Where(u => u.Name.Contains(UserSearchBox.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                UserListView.ItemsSource = new ObservableCollection<UserAccount>(filteredUsers);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
