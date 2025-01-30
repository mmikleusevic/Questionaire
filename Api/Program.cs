using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<QuestionaireDbContext>(options => 
    options.UseSqlServer(Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")));

builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();

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

app.UseHttpsRedirection();

app.UseCors("Cors");

app.UseRouting();

app.MapControllers();

app.Run();