using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using BlazorBootstrap;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json.Linq;
using Web.Interfaces;
using Web.Models;

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
            if (string.IsNullOrEmpty(accessToken))
                return new AuthenticationState(user);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await httpClient.GetAsync("manage/info");
            if (!response.IsSuccessStatusCode)
                return new AuthenticationState(user);

            string responseData = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseData);

            string? email = jsonResponse["email"]?.ToString();
            string? name = jsonResponse["name"]?.ToString();
            JArray? rolesArray = jsonResponse["roles"] as JArray;

            List<Claim> claims = new()
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

            ClaimsIdentity identity = new(claims, "Token");
            user = new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "fetching authentication state", logger);
        }

        return new AuthenticationState(user);
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

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

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

            await authRedirectService.CheckAndRedirect(null);
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
                Email = registerData.Email,
                Password = registerData.Password
            };

            await Login(loginData);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "registering user", logger);
        }
    }
}