using System.Diagnostics;
using System.Net.Http.Json;
using Shared.Models;

namespace Web.Logger;

public class ApiLogger(string categoryName, string logApiEndpoint, IHttpClientFactory httpClientFactory)
    : ILogger
{
    private static DateTime lastFailure = DateTime.MinValue;
    private static readonly TimeSpan SuppressPeriod = TimeSpan.FromMinutes(1);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        string? message = formatter(state, exception);

        LogEntryDto logEntry = new LogEntryDto
        {
            Level = logLevel.ToString(),
            Message = message,
            SourceContext = categoryName,
            ExceptionDetails = exception?.ToString()
        };

        _ = SendLogToServerAsync(logEntry);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Warning;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    private async Task SendLogToServerAsync(LogEntryDto logEntry)
    {
        if (DateTime.UtcNow - lastFailure < SuppressPeriod)
        {
            Debug.WriteLine("[ApiLogger] Skipping log due to recent failure.");
            return;
        }

        try
        {
            HttpClient httpClient = httpClientFactory.CreateClient("WebAPI");

            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(logApiEndpoint, logEntry);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[ApiLogger] Failed to send log entry to API. Status: {response.StatusCode}");
                lastFailure = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            if (ex is HttpRequestException httpEx &&
                httpEx.InnerException?.Message.Contains("CORS", StringComparison.OrdinalIgnoreCase) == true)
            {
                Debug.WriteLine("[ApiLogger] Skipping known CORS-related logging exception.");
                return;
            }

            Debug.WriteLine($"[ApiLogger] Exception occurred while sending log entry to API: {ex.Message}");
            lastFailure = DateTime.UtcNow;
        }
    }
}