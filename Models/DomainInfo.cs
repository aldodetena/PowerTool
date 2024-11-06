namespace PowerTool.Models
{
    /// <summary>
    /// Representa la información de un dominio, incluyendo su nombre, el nombre de usuario y la contraseña cifrada.
    /// </summary>
    public class DomainInfo
    {
        public string? DomainName { get; set; }
        public string? Username { get; set; }
        public string? EncryptedPassword { get; set; }
    }
}
