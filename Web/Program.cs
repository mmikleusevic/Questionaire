using Microsoft.AspNetCore.Components;
using Web.Components;
using Web.Interfaces;
using Web.Models;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

ApiSettings? apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>();

builder.Services.AddLogging();
builder.Services.AddBlazorBootstrap();

builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(apiSettings.BaseUrl)
});
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();