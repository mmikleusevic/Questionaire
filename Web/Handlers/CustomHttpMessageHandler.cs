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

            if (response.StatusCode == HttpStatusCode.Unauthorized && !IsIdentityRequest(request))
            {
                logger.LogInformation("Received 401 Unauthorized for {RequestUri}. Attempting token refresh.",
                    request.RequestUri);

                bool refreshed = await authStateProvider.RefreshTokenAsync();

                if (refreshed)
                {
                    logger.LogInformation("Token refresh successful. Retrying request to {RequestUri}.",
                        request.RequestUri);
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

            try
            {
                using IServiceScope emergencyScope = serviceProvider.CreateScope();
                CustomAuthStateProvider emergencyAuthProvider =
                    emergencyScope.ServiceProvider.GetRequiredService<CustomAuthStateProvider>();
                await emergencyAuthProvider.Logout();
            }
            catch (Exception logoutEx)
            {
                logger.LogError(logoutEx, "Failed to logout user after an exception in CustomHttpMessageService.");
            }

            response?.Dispose();

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "An internal error occurred in the message handler."
            };
        }
    }

    private bool IsIdentityRequest(HttpRequestMessage request)
    {
        return request.RequestUri?.AbsolutePath.EndsWith(LoginPath, StringComparison.OrdinalIgnoreCase) == true
               || request.RequestUri?.AbsolutePath.EndsWith(RegisterPath, StringComparison.OrdinalIgnoreCase) == true;
    }
}