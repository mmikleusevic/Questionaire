using System.Net;
using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace Web.Services;

public class CustomHttpMessageService(
    ILocalStorageService localStorageService,
    IServiceProvider serviceProvider)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CustomAuthStateService authStateService = serviceProvider.GetRequiredService<CustomAuthStateService>();

        try
        {
            string? accessToken = await localStorageService.GetItemAsync<string>("accessToken", cancellationToken);
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool refreshed = await authStateService.RefreshTokenAsync();
                if (refreshed)
                {
                    accessToken = await localStorageService.GetItemAsync<string>("accessToken", cancellationToken);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    await authStateService.Logout();
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            await authStateService.Logout();

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent($"An error occurred while processing the request: {ex.Message}")
            };
        }
    }
}