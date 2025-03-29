namespace Web.Logger;

public static class ApiLoggerExtensions
{
    public static ILoggingBuilder AddApiLogger(this ILoggingBuilder builder, Action<ApiLoggerOptions> configure)
    {
        ApiLoggerOptions options = new ApiLoggerOptions();
        configure(options);

        if (string.IsNullOrWhiteSpace(options.LogApiEndpoint))
        {
            throw new InvalidOperationException("API Logger endpoint must be configured.");
        }

        builder.Services.AddSingleton<ILoggerProvider>(sp =>
            new ApiLoggerProvider(
                sp.GetRequiredService<IHttpClientFactory>(),
                options.LogApiEndpoint
            ));

        return builder;
    }
}