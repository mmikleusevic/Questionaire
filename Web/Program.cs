using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Web;
using Web.Interfaces;
using Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.SetMinimumLevel(LogLevel.Error);
builder.Services.AddLogging();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazorBootstrap();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAuthRedirectService, AuthRedirectService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<JavaScriptService>();
builder.Services.AddScoped<CustomAuthStateService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateService>();
builder.Services.AddTransient<CustomHttpMessageService>();

string? apiUrl = builder.Configuration["ApiSettings:BaseUrl"];

if (string.IsNullOrEmpty(apiUrl))
{
    throw new InvalidOperationException("API URL is missing");
}

Uri uri = new Uri(apiUrl);

builder.Services.AddHttpClient("WebAPI", client => { client.BaseAddress = uri; })
    .AddHttpMessageHandler<CustomHttpMessageService>();

builder.Services
    .AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
        .CreateClient("WebAPI"));

await builder.Build().RunAsync();