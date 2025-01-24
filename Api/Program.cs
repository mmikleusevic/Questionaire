using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Services;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Routing.Constraints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<QuestionaireDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();

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