using System.Security.Claims;
using BlazorBootstrap;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Web.Interfaces;

namespace Web.Services;

public class AuthRedirectService(
    NavigationManager navigationManager,
    ToastService toastService,
    ILogger<AuthRedirectService> logger,
    ILocalStorageService localStorageService) : IAuthRedirectService
{
    private static readonly Dictionary<string, string> RoleRedirects = new Dictionary<string, string>
    {
        { "Admin", "/Questions" },
        { "User", "/PendingQuestions" },
        { "SuperAdmin", "/Users" }
    };

    public async Task CheckAndRedirect(ClaimsPrincipal? user = null)
    {
        try
        {
            string? token = await localStorageService.GetItemAsync<string>("accessToken");
        
            if (string.IsNullOrEmpty(token))
            {
                navigationManager.NavigateTo("/Login", true);
                return;
            }
            
            if (user != null)
            {
                foreach (KeyValuePair<string, string> roleRedirect in RoleRedirects)
                {
                    if (user.IsInRole(roleRedirect.Key))
                    {
                        navigationManager.NavigateTo(roleRedirect.Value, true);
                        return;
                    }
                }
            }
            
            navigationManager.NavigateTo("/Login", true);
        }
        catch (Exception ex)
        {
            ApiResponseHandler.HandleException(ex, toastService, "redirecting user", logger);
        }
    }
}