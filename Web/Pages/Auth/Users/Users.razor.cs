using System.Security.Claims;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Models;
using Web.Interfaces;
using Web.Pages.Auth.Users.UserModals;
using UpdateUser = Web.Pages.Auth.Users.UserModals.UpdateUser;

namespace Web.Pages.Auth.Users;

public partial class Users : ComponentBase
{
    private Modal? modal;
    private IList<RoleDto>? roles;
    private List<UserDto>? users;
    private ClaimsPrincipal currentUser;
    
    [CascadingParameter] Task<AuthenticationState> authenticationStateTask { get; set; }
    [Inject] private IRoleService? RoleService { get; set; }
    [Inject] private IUserService? UserService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetUsers();
        await GetRoles();
        
        AuthenticationState authState = await authenticationStateTask;
        currentUser = authState.User;

        await base.OnInitializedAsync();
    }

    private async Task ShowUpdateUser(UserDto? userDto)
    {
        if (modal == null || userDto == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "User", userDto },
            { "Roles", roles },
            { "OnUserChanged", EventCallback.Factory.Create(this, GetUsers) },
            { "Modal", modal }
        };

        await modal.ShowAsync<UpdateUser>("Update User", parameters: parameters);
    }

    private async Task ShowDeleteUser(UserDto? userDto)
    {
        if (modal == null || userDto == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", modal },
            { "User", userDto },
            { "OnUserChanged", EventCallback.Factory.Create(this, GetUsers) }
        };

        await modal.ShowAsync<DeleteUser>("Delete User", parameters: parameters);
    }

    private async Task GetUsers()
    {
        if (UserService == null) return;

        users = await UserService.GetUsers();
    }

    private async Task GetRoles()
    {
        if (RoleService == null) return;

        roles = await RoleService.GetRoles();
    }
    
    private bool IsDeleteDisabled(UserDto user)
    {
        bool isSuperAdmin = user.Roles.Any(r => r.RoleName.Equals("Superadmin", StringComparison.OrdinalIgnoreCase));
        bool isSelf = currentUser?.Identity?.Name?.Equals(user.UserName, StringComparison.OrdinalIgnoreCase) ?? false;

        return isSuperAdmin || isSelf;
    }
}