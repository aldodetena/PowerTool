using System.Collections.ObjectModel;
using System.Windows;
using System.DirectoryServices.AccountManagement;
using PowerTool.Models;
using PowerTool.Utilities;
using PowerTool.Services;

namespace PowerTool.Views
{
    /// <summary>
    /// Ventana de gestión de usuarios del dominio.
    /// Permite listar, habilitar/deshabilitar, desbloquear y restablecer contraseñas de cuentas de usuario.
    /// </summary>
    public partial class UserManagementWindow : Window
    {
        /// <summary>
        /// Información del dominio seleccionado utilizada para las operaciones con usuarios.
        /// </summary>
        private DomainInfo _selectedDomain;
        /// <summary>
        /// Colección observable de cuentas de usuario obtenidas del dominio.
        /// Permite la actualización dinámica de la interfaz de usuario.
        /// </summary>
        public ObservableCollection<UserAccount> UserAccounts { get; set; }

        /// <summary>
        /// Constructor que inicializa la ventana con el dominio seleccionado y carga los usuarios del mismo.
        /// </summary>
        /// <param name="selectedDomain">Información del dominio seleccionado.</param>
        public UserManagementWindow(DomainInfo selectedDomain)
        {
            InitializeComponent();
            _selectedDomain = selectedDomain;
            UserAccounts = new ObservableCollection<UserAccount>();
            DataContext = this;
            LoadUsers();
        }

        /// <summary>
        /// Carga y lista las cuentas de usuario del dominio.
        /// </summary>
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

        /// <summary>
        /// Cambia el estado de activación de un usuario seleccionado (activar/desactivar).
        /// </summary>
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

        /// <summary>
        /// Desbloquea la cuenta de un usuario bloqueado.
        /// </summary>
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

        /// <summary>
        /// Restablece la contraseña de un usuario seleccionado tras solicitar la nueva contraseña.
        /// </summary>
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

        /// <summary>
        /// Filtra la lista de usuarios según el texto ingresado en el cuadro de búsqueda.
        /// </summary>
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

        /// <summary>
        /// Cierra la ventana de gestión de usuarios.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
