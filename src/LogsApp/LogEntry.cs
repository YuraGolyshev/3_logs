namespace LogsApp;

internal class LogEntry
{
    public string RemoteAddr { get; set; } = "";
    public string RemoteUser { get; set; } = "";
    public DateTime TimeLocal { get; set; }
    public string Resource { get; set; } = "";
    public string Protocol { get; set; } = "";
    public int Status { get; set; }
    public int BodyBytesSent { get; set; }
    public string Referer { get; set; } = "";
    public string UserAgent { get; set; } = "";
}
