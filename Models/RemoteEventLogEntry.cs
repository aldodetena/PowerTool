namespace PowerTool.Models
{
    public class RemoteEventLogEntry
{
    public string? Source { get; set; }
    public string? EventType { get; set; }
    public DateTime TimeGenerated { get; set; }
    public string? Message { get; set; }
}
}