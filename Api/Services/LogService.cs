using QuestionaireApi.Interfaces;
using Shared.Models;

namespace QuestionaireApi.Services;

public class LogService(ILogger<LogService> logger) : ILogService
{
    public void LogClientEntry(LogEntryDto logEntry)
    {
        LogLevel logLevel = logEntry.Level?.ToUpperInvariant() switch
        {
            "TRACE" => LogLevel.Trace,
            "DEBUG" => LogLevel.Debug,
            "INFORMATION" => LogLevel.Information,
            "WARNING" => LogLevel.Warning,
            "ERROR" => LogLevel.Error,
            "CRITICAL" => LogLevel.Critical,
            _ => LogLevel.Warning
        };

        string messageTemplate = "Client Log [{ClientSourceContext}]: {ClientMessage}";

        if (!string.IsNullOrEmpty(logEntry.ExceptionDetails))
        {
            messageTemplate += "\nClient Exception: {ClientException}";
            logger.Log(logLevel, messageTemplate, logEntry.SourceContext ?? "WASM", logEntry.Message,
                logEntry.ExceptionDetails);
        }
        else
        {
            logger.Log(logLevel, messageTemplate, logEntry.SourceContext ?? "WASM", logEntry.Message);
        }
    }
}