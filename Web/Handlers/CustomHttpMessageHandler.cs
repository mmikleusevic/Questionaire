using System.Net;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Web.Providers;

namespace Web.Handlers;

public class CustomHttpMessageHandler(
    ILocalStorageService localStorageService,
    IServiceProvider serviceProvider,
    ILogger<CustomHttpMessageHandler> logger)
    : DelegatingHandler
{
    private const string AccessTokenKey = "accessToken";
    private const string BearerScheme = "Bearer";
    private const string LoginPath = "/login";
    private const string RegisterPath = "/register";
    private const string RefreshPath = "/refresh";
    private const string XRetryAttempt = "X-Retry-Attempt";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = null;

        using IServiceScope scope = serviceProvider.CreateScope();
        CustomAuthStateProvider authStateProvider = scope.ServiceProvider.GetRequiredService<CustomAuthStateProvider>();

        try
        {
            string? accessToken = await localStorageService.GetItemAsync<string>(AccessTokenKey, cancellationToken);
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, accessToken);
            }

            response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized
                && !IsIdentityRequest(request)
                && !request.Headers.Contains(XRetryAttempt))
            {
                bool refreshed = await authStateProvider.RefreshTokenAsync();

                if (refreshed)
                {
                    response.Dispose();

                    accessToken = await localStorageService.GetItemAsync<string>(AccessTokenKey, cancellationToken);
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, accessToken);
                    }
                    else
                    {
                        logger.LogWarning("Token refresh reported success, but no new access token found in storage.");
                    }

                    request.Headers.Add(XRetryAttempt, "1");

                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    logger.LogWarning("Token refresh failed. Logging out user.");
                    await authStateProvider.Logout();
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "An unexpected error occurred in CustomHttpMessageService while processing request to {RequestUri}.",
                request?.RequestUri);

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "An internal error occurred in the message handler."
            };
        }
    }

    private bool IsIdentityRequest(HttpRequestMessage request)
    {
        return request.RequestUri?.AbsolutePath.EndsWith(LoginPath, StringComparison.OrdinalIgnoreCase) == true
               || request.RequestUri?.AbsolutePath.EndsWith(RegisterPath, StringComparison.OrdinalIgnoreCase) == true
               || request.RequestUri?.AbsolutePath.EndsWith(RefreshPath, StringComparison.OrdinalIgnoreCase) == true;
    }
}