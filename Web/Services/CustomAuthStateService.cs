using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using BlazorBootstrap;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Web.Models;

namespace Web.Services;

public class CustomAuthStateService(
    HttpClient httpClient,
    ILogger<CustomAuthStateService> logger,
    ToastService toastService,
    NavigationManager navigationManager,
    ILocalStorageService localStorageService)
    : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity());

        try
        {
            string? accessToken = await localStorageService.GetItemAsync<string>("accessToken");
            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage? response = await httpClient.GetAsync("manage/info");
                if (response.IsSuccessStatusCode)
                {
                    string? responseData = await response.Content.ReadAsStringAsync();
                    JObject? jsonResponse = JObject.Parse(responseData);

                    string? email = jsonResponse?["email"]?.ToString();
                    string? name = jsonResponse?["name"]?.ToString();
                    JArray? rolesArray = jsonResponse?["roles"] as JArray;

                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, email ?? string.Empty),
                        new Claim(ClaimTypes.Name, name ?? string.Empty),
                    };

                    if (rolesArray != null)
                    {
                        foreach (JToken role in rolesArray)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                        }
                    }

                    ClaimsIdentity identity = new ClaimsIdentity(claims, "Token");
                    user = new ClaimsPrincipal(identity);
                }
            }
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error while fetching authentication state.", ex.Message);
            logger.LogError(ex, "Error while fetching authentication state.");
        }
        
        return new AuthenticationState(user);
    }

    public async Task Login(LoginData loginData)
    {
        try
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("login", new { email = loginData.Username, password = loginData.Password });

            if (response.IsSuccessStatusCode)
            {
                string? responseData = await response.Content.ReadAsStringAsync();
                JObject? jsonResponse = JObject.Parse(responseData);
                string? accessToken = jsonResponse?["accessToken"]?.ToString();
                string? refreshToken = jsonResponse?["refreshToken"]?.ToString();
                
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                
                await localStorageService.SetItemAsync("accessToken", accessToken);
                await localStorageService.SetItemAsync("refreshToken", refreshToken);
                
                AuthenticationState authState = await GetAuthenticationStateAsync();
                NotifyAuthenticationStateChanged(Task.FromResult(authState));

                if (authState.User.IsInRole("SuperAdmin") || authState.User.IsInRole("Admin"))
                {
                    navigationManager.NavigateTo("/Questions", true);
                }
                else if (authState.User.IsInRole("User"))
                {
                    navigationManager.NavigateTo("/PendingQuestions", true);
                }

                Helper.ShowToast(toastService, response.StatusCode, "User successfully logged in", "User successfully logged in");
                return;
            }
            
            Helper.ShowToast(toastService, response.StatusCode, "Incorrect Username or Password", "Incorrect Email or Password");
        }
        catch (Exception ex)
        {
            Helper.ShowToast(toastService, HttpStatusCode.InternalServerError, "Error while logging user in.", ex.Message);
            logger.LogError(ex, "Error while logging user in.");
        }
    }
    
    public async Task Logout()
    {
        await localStorageService.RemoveItemAsync("accessToken");
        await localStorageService.RemoveItemAsync("refreshToken");
        
        httpClient.DefaultRequestHeaders.Remove("Authorization");
        
        AuthenticationState authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
        
        navigationManager.NavigateTo("/", true);
    }
}