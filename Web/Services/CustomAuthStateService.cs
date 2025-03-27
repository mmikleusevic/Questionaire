using System.Net.Http.Json;
using System.Security.Claims;
using BlazorBootstrap;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json.Linq;
using Shared.Models;
using Web.Interfaces;

namespace Web.Services;

public class CustomAuthStateService(
    HttpClient httpClient,
    ILogger<CustomAuthStateService> logger,
    ToastService toastService,
    ILocalStorageService localStorageService,
    IAuthRedirectService authRedirectService)
    : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsPrincipal user = anonymousUser;

        try
        {
            string? accessToken = await localStorageService.GetItemAsync<string>("accessToken");

            if (string.IsNullOrEmpty(accessToken)) return new AuthenticationState(user);

            HttpResponseMessage response = await httpClient.GetAsync("manage/info");

            if (!response.IsSuccessStatusCode) return new AuthenticationState(user);

            string responseData = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseData);

            user = CreateUserFromJson(jsonResponse);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching authentication state", logger);
        }

        return new AuthenticationState(user);
    }

    private ClaimsPrincipal CreateUserFromJson(JObject jsonResponse)
    {
        string? email = jsonResponse["email"]?.ToString();
        string? name = jsonResponse["name"]?.ToString();
        JArray? rolesArray = jsonResponse["roles"] as JArray;

        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email ?? string.Empty),
            new Claim(ClaimTypes.Name, name ?? string.Empty)
        };

        if (rolesArray != null)
        {
            foreach (JToken role in rolesArray)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }
        }

        ClaimsIdentity identity = new ClaimsIdentity(claims, "Token");
        return new ClaimsPrincipal(identity);
    }

    public async Task Login(LoginData loginData)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("login", loginData);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, "logging user in")) return;

            string responseData = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseData);

            string? accessToken = jsonResponse["accessToken"]?.ToString();
            string? refreshToken = jsonResponse["refreshToken"]?.ToString();

            await localStorageService.SetItemAsync("accessToken", accessToken);
            await localStorageService.SetItemAsync("refreshToken", refreshToken);

            AuthenticationState authState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authState));

            await authRedirectService.CheckAndRedirect(authState.User);

            ToastHandler.ShowToast(toastService, response.StatusCode, "User successfully logged in",
                "User successfully logged in");
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "logging user in", logger);
        }
    }

    public async Task Logout()
    {
        try
        {
            await localStorageService.RemoveItemAsync("accessToken");
            await localStorageService.RemoveItemAsync("refreshToken");

            httpClient.DefaultRequestHeaders.Remove("Authorization");

            AuthenticationState authState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authState));

            await authRedirectService.CheckAndRedirect(anonymousUser);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "logging user out", logger);
        }
    }

    public async Task Register(RegisterData registerData)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("register", registerData);

            if (!await ApiResponseHandler.HandleResponse(response, toastService, "registering user")) return;

            LoginData loginData = new LoginData
            {
                UserName = registerData.UserName,
                Password = registerData.Password
            };

            await Login(loginData);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "registering user", logger);
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            string? refreshToken = await localStorageService.GetItemAsync<string>("refreshToken");

            if (string.IsNullOrEmpty(refreshToken)) return false;

            HttpResponseMessage response =
                await httpClient.PostAsJsonAsync("refresh", new { RefreshToken = refreshToken });

            if (!response.IsSuccessStatusCode) return false;

            string responseData = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseData);

            string? newAccessToken = jsonResponse["accessToken"]?.ToString();
            string? newRefreshToken = jsonResponse["refreshToken"]?.ToString();

            if (string.IsNullOrEmpty(newAccessToken) || string.IsNullOrEmpty(newRefreshToken)) return false;

            await localStorageService.SetItemAsync("accessToken", newAccessToken);
            await localStorageService.SetItemAsync("refreshToken", newRefreshToken);

            return true;
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "refreshing token", logger);
            return false;
        }
    }
}