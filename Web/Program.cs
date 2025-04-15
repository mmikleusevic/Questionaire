using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Web;
using Web.Handlers;
using Web.Interfaces;
using Web.Logger;
using Web.Providers;
using Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazorBootstrap();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAuthRedirectService, AuthRedirectService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserQuestionHistoryService, UserQuestionHistoryService>();
builder.Services.AddScoped<JavaScriptHandler>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddTransient<CustomHttpMessageHandler>();

string? apiUrl = builder.Configuration["ApiSettings:BaseUrl"];

if (string.IsNullOrEmpty(apiUrl))
{
    throw new InvalidOperationException("API URL 'ApiSettings:BaseUrl' is missing in configuration.");
}

Uri uri = new Uri(apiUrl);

builder.Services.AddHttpClient("WebAPI", client => { client.BaseAddress = uri; })
    .AddHttpMessageHandler<CustomHttpMessageHandler>();

builder.Services
    .AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
        .CreateClient("WebAPI"));

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Logging.AddApiLogger(options => { options.LogApiEndpoint = "api/logs"; });

await builder.Build().RunAsync();