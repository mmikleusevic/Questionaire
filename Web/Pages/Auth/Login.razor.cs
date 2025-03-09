using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Models;
using Web.Services;

namespace Web.Pages.Auth;

public partial class Login : ComponentBase
{
    [Inject] CustomAuthStateService CustomAuthStateService { get; set; }
    
    private readonly LoginData loginData = new LoginData();
    private EditContext? editContext;

    protected override async Task OnInitializedAsync()
    {
        loginData.Username = string.Empty;
        loginData.Password = string.Empty;
        
        editContext = new EditContext(loginData);
        
        await base.OnInitializedAsync();
    }
    
    public async Task HandleValidSubmit()
    {
        await CustomAuthStateService.Login(loginData);
    }
}