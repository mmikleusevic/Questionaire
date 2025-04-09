using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using BlazorBootstrap;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Models;
using Web.Handlers;
using Web.Interfaces;

namespace Web.Providers;

public class CustomAuthStateProvider(
    HttpClient httpClient,
    ILogger<CustomAuthStateProvider> logger,
    ToastService toastService,
    ILocalStorageService localStorageService,
    IAuthRedirectService authRedirectService)
    : AuthenticationStateProvider
{
    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";
    private const string ManageInfoEndpoint = "manage/info";
    private const string LoginEndpoint = "login";
    private const string RegisterEndpoint = "register";
    private const string RefreshEndpoint = "refresh";
    private const string AuthScheme = "Token";

    private readonly ClaimsPrincipal anonymousUser = new(new ClaimsIdentity());

    /// <summary>
    ///     Gets the current authentication state, fetching user info if tokens are present.
    ///     Returns anonymous state if tokens are missing or user info fetch fails.
    ///     Avoids showing generic API error toasts for internal state checks.
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsPrincipal user = anonymousUser;

        try
        {
            string? accessToken = await localStorageService.GetItemAsync<string>(AccessTokenKey);
            if (string.IsNullOrEmpty(accessToken))
            {
                logger.LogDebug("No access token found in local storage.");
                return new AuthenticationState(anonymousUser);
            }

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ManageInfoEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Fetching user info from {Endpoint} failed with status {StatusCode}. Treating user as anonymous.",
                    ManageInfoEndpoint, response.StatusCode);
                return new AuthenticationState(anonymousUser);
            }

            string responseData = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseData);
            user = CreateUserFromJson(jsonResponse);
        }
        catch (JsonReaderException jsonEx)
        {
            logger.LogError(jsonEx, "Failed to parse JSON response during authentication state check.");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching authentication state", logger);
        }

        return new AuthenticationState(user);
    }

    /// <summary>
    ///     Creates a ClaimsPrincipal from a JObject containing user info.
    /// </summary>
    private ClaimsPrincipal CreateUserFromJson(JObject jsonResponse)
    {
        string? email = jsonResponse.Value<string>("email");
        string? name = jsonResponse.Value<string>("name");
        string? userId = jsonResponse.Value<string>("userId");
        JArray? rolesArray = jsonResponse["roles"] as JArray;

        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(email)) claims.Add(new Claim(ClaimTypes.Email, email));
        if (!string.IsNullOrEmpty(name)) claims.Add(new Claim(ClaimTypes.Name, name));
        if (!string.IsNullOrEmpty(userId)) claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

        if (rolesArray != null)
        {
            foreach (JToken role in rolesArray)
            {
                string? roleName = role?.Value<string>();
                if (!string.IsNullOrEmpty(roleName))
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
                }
            }
        }

        if (!claims.Any())
        {
            logger.LogWarning("Could not extract any claims from user info JSON.");
            return anonymousUser;
        }

        ClaimsIdentity identity = new ClaimsIdentity(claims, AuthScheme);
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    ///     Attempts to log the user in using provided credentials.
    ///     Handles API response and updates authentication state.
    /// </summary>
    public async Task Login(LoginData loginData)
    {
        string context = "logging user in";
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(LoginEndpoint, loginData);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;

            string responseData = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseData);

            string? accessToken = jsonResponse.Value<string>("accessToken");
            string? refreshToken = jsonResponse.Value<string>("refreshToken");

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                logger.LogError("Login response missing access or refresh token.");
                ToastHandler.ShowToast(toastService, HttpStatusCode.InternalServerError, "Login Error",
                    "Received invalid token response from server.");
                return;
            }

            await localStorageService.SetItemAsync(AccessTokenKey, accessToken);
            await localStorageService.SetItemAsync(RefreshTokenKey, refreshToken);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            AuthenticationState newAuthState = await GetAuthenticationStateAsync();
            await authRedirectService.CheckAndRedirect(newAuthState.User);
        }
        catch (JsonReaderException jsonEx)
        {
            logger.LogError(jsonEx, "Failed to parse JSON response during {Context}.", context);
            ToastHandler.ShowToast(toastService, HttpStatusCode.InternalServerError, "Login Error",
                "Failed to process login response.");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }

    /// <summary>
    ///     Logs the user out by clearing tokens and notifying the application.
    /// </summary>
    public async Task Logout()
    {
        string context = "logging user out";
        try
        {
            await localStorageService.RemoveItemAsync(AccessTokenKey);
            await localStorageService.RemoveItemAsync(RefreshTokenKey);

            logger.LogInformation("User logged out, clearing tokens.");

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));

            await authRedirectService.CheckAndRedirect(anonymousUser);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }

    /// <summary>
    ///     Attempts to register a new user and then log them in.
    /// </summary>
    public async Task Register(RegisterData registerData)
    {
        string context = "registering user";
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(RegisterEndpoint, registerData);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, context, logger)) return;

            logger.LogInformation("Registration successful for user {UserName}. Attempting login.",
                registerData.UserName);
            var loginData = new LoginData
            {
                UserName = registerData.UserName,
                Password = registerData.Password
            };

            await Login(loginData);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
        }
    }

    /// <summary>
    ///     Attempts to refresh the access token using the stored refresh token.
    ///     Avoids showing generic API error toasts for this internal background operation.
    /// </summary>
    /// <returns>True if tokens were successfully refreshed, otherwise false.</returns>
    public async Task<bool> RefreshTokenAsync()
    {
        string context = "refreshing token";
        try
        {
            string? refreshToken = await localStorageService.GetItemAsync<string>(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
            {
                logger.LogDebug("No refresh token available for token refresh.");
                return false;
            }

            HttpResponseMessage response =
                await httpClient.PostAsJsonAsync(RefreshEndpoint, new { RefreshToken = refreshToken });

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Token refresh request failed with status {StatusCode}.", response.StatusCode);
                return false;
            }

            string responseData = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseData);

            string? newAccessToken = jsonResponse.Value<string>("accessToken");
            string? newRefreshToken = jsonResponse.Value<string>("refreshToken");

            if (string.IsNullOrEmpty(newAccessToken) || string.IsNullOrEmpty(newRefreshToken))
            {
                logger.LogError("Token refresh response missing new access or refresh token.");
                return false;
            }

            await localStorageService.SetItemAsync(AccessTokenKey, newAccessToken);
            await localStorageService.SetItemAsync(RefreshTokenKey, newRefreshToken);

            logger.LogInformation("Access token successfully refreshed.");
            return true;
        }
        catch (JsonReaderException jsonEx)
        {
            logger.LogError(jsonEx, "Failed to parse JSON response during {Context}.", context);
            return false;
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, context, logger);
            return false;
        }
    }
}