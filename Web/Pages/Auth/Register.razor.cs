using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Models;
using Web.Services;

namespace Web.Pages.Auth;

public partial class Register : ComponentBase
{
    private readonly RegisterData registerData = new RegisterData();
    private EditContext? editContext;
    [Inject] private CustomAuthStateService CustomAuthStateService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        registerData.Email = string.Empty;
        registerData.Password = string.Empty;
        registerData.ConfirmPassword = string.Empty;

        editContext = new EditContext(registerData);

        await base.OnInitializedAsync();
    }

    public async Task HandleValidSubmit()
    {
        await CustomAuthStateService.Register(registerData);
    }
}