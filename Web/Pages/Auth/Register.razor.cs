using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Models;
using Web.Providers;

namespace Web.Pages.Auth;

public partial class Register : ComponentBase
{
    private readonly RegisterData registerData = new RegisterData();
    private EditContext? editContext;
    [Inject] private CustomAuthStateProvider CustomAuthStateProvider { get; set; }

    protected override async Task OnInitializedAsync()
    {
        registerData.UserName = string.Empty;
        registerData.Password = string.Empty;
        registerData.ConfirmPassword = string.Empty;

        editContext = new EditContext(registerData);

        await base.OnInitializedAsync();
    }

    public async Task HandleValidSubmit()
    {
        await CustomAuthStateProvider.Register(registerData);
    }
}