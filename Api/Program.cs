using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuestionaireApi;
using QuestionaireApi.IdentityApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Services;
using Scalar.AspNetCore;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

builder.Services.AddLogging();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

string connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

if (string.IsNullOrWhiteSpace(connectionString)) return;

builder.Services.AddDbContext<QuestionaireDbContext>(options =>
{
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    options.UseSqlServer(Environment.GetEnvironmentVariable("DEFAULT_CONNECTION"));
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.BearerScheme;
    options.DefaultSignInScheme = IdentityConstants.BearerScheme;
    options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
})
.AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IUserQuestionHistoryService, UserQuestionHistoryService>();
builder.Services.AddScoped<IPendingQuestionService, PendingQuestionService>();
builder.Services.AddScoped<IQuestionCategoriesService, QuestionCategoriesService>();
builder.Services.AddScoped<IPendingAnswerService, PendingAnswerService>();
builder.Services.AddScoped<IPendingQuestionCategoriesService, PendingQuestionCategoriesService>();

string applicationUrl = Environment.GetEnvironmentVariable("API_URL");
string webUrl = Environment.GetEnvironmentVariable("WEB_URL");

if (!string.IsNullOrEmpty(applicationUrl))
{
    builder.WebHost.UseUrls(applicationUrl);
}

builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddApiEndpoints()
    .AddEntityFrameworkStores<QuestionaireDbContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "Cors", policy =>
    {
        policy.WithOrigins(applicationUrl,webUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsJsonAsync("An application error has occurred. Try again later.");
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

using (IServiceScope? scope = app.Services.CreateScope())
{
    QuestionaireDbContext dbContext = scope.ServiceProvider.GetRequiredService<QuestionaireDbContext>();
    
    dbContext.Database.Migrate();
}

app.CustomMapIdentityApi<User>();

app.UseHttpsRedirection();

app.UseCors("Cors");

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();