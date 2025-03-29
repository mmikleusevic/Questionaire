namespace Shared.Models;

public class LogEntryDto
{
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ExceptionDetails { get; set; }
    public string? SourceContext { get; set; }
}