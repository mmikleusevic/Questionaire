using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestionaireApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

// Add services to the container.

builder.Services.AddLogging();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<QuestionaireDbContext>(options => 
    options.UseSqlServer(Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")));

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<QuestionaireDbContext>();

builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IUserQuestionHistoryService, UserQuestionHistoryService>();
builder.Services.AddScoped<IPendingQuestionService, PendingQuestionService>();
builder.Services.AddScoped<IQuestionCategoriesService, QuestionCategoriesService>();
builder.Services.AddScoped<IPendingAnswerService, PendingAnswerService>();
builder.Services.AddScoped<IPendingQuestionCategoriesService, PendingQuestionCategoriesService>();

string applicationUrl = Environment.GetEnvironmentVariable("APPLICATION_URL");

if (!string.IsNullOrEmpty(applicationUrl))
{
    builder.WebHost.UseUrls(applicationUrl);
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "Cors", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyOrigin()
            .AllowAnyMethod();
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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<QuestionaireDbContext>();
    dbContext.Database.Migrate();
}

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

app.MapIdentityApi<IdentityUser>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("Cors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();