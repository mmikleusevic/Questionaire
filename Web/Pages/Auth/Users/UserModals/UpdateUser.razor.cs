using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Auth.Users.UserModals;

public partial class UpdateUser : ComponentBase
{
    private EditContext? editContext;
    private List<RoleDto> selectedRoles = new List<RoleDto>();
    private UserDto updatedUser = new UserDto();

    private List<string> validationMessages = new List<string>();
    [Inject] private IUserService? UserService { get; set; }
    [Parameter] public UserDto? User { get; set; }
    [Parameter] public List<RoleDto>? Roles { get; set; }
    [Parameter] public EventCallback OnUserChanged { get; set; }
    [Parameter] public Modal? Modal { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        updatedUser = new UserDto
        {
            UserName = User?.UserName ?? string.Empty,
            Email = User?.Email ?? string.Empty,
            Roles = User?.Roles.ToList() ?? new List<RoleDto>()
        };

        selectedRoles = updatedUser.Roles.ToList();

        validationMessages.Clear();
        editContext = new EditContext(updatedUser);
    }

    public async Task HandleValidSubmit()
    {
        if (UserService == null) return;

        List<string> errorMessages = new List<string>();

        bool hasNoRole = updatedUser.Roles.Count == 0;
        if (hasNoRole)
        {
            errorMessages.Add("You have to assign at least one role to the user.");
        }

        if (errorMessages.Any())
        {
            validationMessages = errorMessages;
            return;
        }

        updatedUser.Roles = selectedRoles.ToList();

        User.Roles = updatedUser.Roles;

        await UserService.UpdateUser(User);
        await OnUserChanged.InvokeAsync();
        await Hide();
    }

    private void RemoveRole(RoleDto selectedRole)
    {
        selectedRoles.Remove(selectedRole);
    }

    private void SelectRole(RoleDto role)
    {
        selectedRoles.Add(role);
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}