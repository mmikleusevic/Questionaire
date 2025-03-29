using System.Collections.Concurrent;

namespace Web.Logger;

public class ApiLoggerProvider(
    IHttpClientFactory httpClientFactory,
    string logApiEndpoint)
    : ILoggerProvider
{
    private readonly string logApiEndpoint = logApiEndpoint.TrimStart('/');
    private readonly ConcurrentDictionary<string, ApiLogger> loggers = new ConcurrentDictionary<string, ApiLogger>();

    public ILogger CreateLogger(string categoryName)
    {
        return loggers.GetOrAdd(categoryName, name => new ApiLogger(name, logApiEndpoint, httpClientFactory));
    }

    public void Dispose()
    {
        loggers.Clear();
        GC.SuppressFinalize(this);
    }
}