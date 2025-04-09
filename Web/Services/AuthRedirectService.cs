using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Web.Interfaces;

namespace Web.Services;

public class AuthRedirectService(
    NavigationManager navigationManager,
    ILogger<AuthRedirectService> logger,
    ILocalStorageService localStorageService) : IAuthRedirectService
{
    private const string SuperAdminRole = "SuperAdmin";
    private const string AdminRole = "Admin";
    private const string UserRole = "User";
    private const string AccessTokenKey = "accessToken";
    private const string LoginPath = "/Login";

    private static readonly List<KeyValuePair<string, string>> RoleRedirects = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>(SuperAdminRole, "/Users"),
        new KeyValuePair<string, string>(AdminRole, "/Questions"),
        new KeyValuePair<string, string>(UserRole, "/PendingQuestions")
    };

    /// <summary>
    ///     Checks authentication status and redirects the user based on roles.
    ///     If no token is found, or if a token exists but user details are missing/invalid,
    ///     or if no matching role redirect is found, redirects to the login page.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal representing the authenticated user. Can be null.</param>
    public async Task CheckAndRedirect(ClaimsPrincipal? user = null)
    {
        string context = "checking auth state and redirecting user";
        try
        {
            string? token = await localStorageService.GetItemAsync<string>(AccessTokenKey);

            if (string.IsNullOrEmpty(token))
            {
                logger.LogInformation("No access token found. Redirecting to {LoginPath}.", LoginPath);
                navigationManager.NavigateTo(LoginPath, true);
                return;
            }

            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                logger.LogWarning(
                    "Access token found, but user principal is null or not authenticated. Redirecting to {LoginPath}.",
                    LoginPath);
                navigationManager.NavigateTo(LoginPath, true);
                return;
            }

            logger.LogInformation("User {UserName} is authenticated. Checking roles for redirect.",
                user.Identity.Name ?? "[Unknown]");
            foreach (KeyValuePair<string, string> roleRedirect in RoleRedirects)
            {
                if (user.IsInRole(roleRedirect.Key))
                {
                    logger.LogInformation("User has role '{Role}'. Redirecting to {Path}.", roleRedirect.Key,
                        roleRedirect.Value);
                    navigationManager.NavigateTo(roleRedirect.Value, true);
                    return;
                }
            }

            logger.LogWarning(
                "User {UserName} is authenticated but has no matching role for redirection ({Roles}). Redirecting to {LoginPath}.",
                user.Identity.Name ?? "[Unknown]",
                string.Join(",", user.FindAll(ClaimTypes.Role).Select(c => c.Value)),
                LoginPath);
            navigationManager.NavigateTo(LoginPath, true);
        }
        catch (Exception ex)
        {
            try
            {
                logger.LogWarning("Attempting fallback redirect to {LoginPath} after exception during redirect check.",
                    LoginPath);
                navigationManager.NavigateTo(LoginPath, true);
            }
            catch (Exception navEx)
            {
                logger.LogError(navEx, "Failed to execute fallback redirect to {LoginPath}.", LoginPath);
            }
        }
    }
}