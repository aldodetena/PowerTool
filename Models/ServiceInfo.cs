namespace PowerTool.Models
{
    /// <summary>
    /// Proporciona información sobre un servicio en ejecución en un equipo, como su nombre, estado y modo de inicio.
    /// </summary>
    public class ServiceInfo
    {
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? State { get; set; }
        public string? StartMode { get; set; }
    }
}
