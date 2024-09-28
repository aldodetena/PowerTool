using System.Security.Cryptography;
using System.Text;

namespace PowerTool.Services
{
    public static class EncryptionHelper
    {
        public static string EncryptString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        public static string DecryptString(string encryptedInput)
        {
            byte[] bytes = Convert.FromBase64String(encryptedInput);
            byte[] decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
