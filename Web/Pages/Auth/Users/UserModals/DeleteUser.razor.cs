using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Auth.Users.UserModals;

public partial class DeleteUser : ComponentBase
{
    [Inject] private IUserService? UserService { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public UserDto? User { get; set; }
    [Parameter] public EventCallback OnUserChanged { get; set; }

    private async Task Delete()
    {
        if (UserService == null) return;

        await UserService.DeleteUser(User.Email);
    }
}