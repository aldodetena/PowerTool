using System.Security.Cryptography;
using System.Text;

namespace PowerTool.Services
{
    /// <summary>
    /// Proporciona métodos para cifrar y descifrar cadenas de texto utilizando la protección de datos del usuario actual.
    /// </summary>
    public static class EncryptionHelper
    {
        /// <summary>
        /// Cifra una cadena de texto utilizando DataProtectionScope.CurrentUser.
        /// </summary>
        /// <param name="input">El texto en claro que se desea cifrar.</param>
        /// <returns>La cadena cifrada en formato Base64.</returns>
        public static string EncryptString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Descifra una cadena de texto previamente cifrada con EncryptString.
        /// </summary>
        /// <param name="encryptedInput">El texto cifrado en formato Base64.</param>
        /// <returns>El texto descifrado en claro.</returns>
        public static string DecryptString(string encryptedInput)
        {
            byte[] bytes = Convert.FromBase64String(encryptedInput);
            byte[] decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
