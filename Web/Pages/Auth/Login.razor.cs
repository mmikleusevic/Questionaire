using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Models;
using Web.Providers;

namespace Web.Pages.Auth;

public partial class Login : ComponentBase
{
    private readonly LoginData loginData = new LoginData();
    private EditContext? editContext;
    [Inject] private CustomAuthStateProvider CustomAuthStateProvider { get; set; }

    protected override async Task OnInitializedAsync()
    {
        loginData.UserName = string.Empty;
        loginData.Password = string.Empty;

        editContext = new EditContext(loginData);

        await base.OnInitializedAsync();
    }

    public async Task HandleValidSubmit()
    {
        await CustomAuthStateProvider.Login(loginData);
    }
}