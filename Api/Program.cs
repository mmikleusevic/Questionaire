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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();