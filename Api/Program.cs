using System.Threading.RateLimiting;
using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QuestionaireApi;
using QuestionaireApi.IdentityApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Services;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Env.Load();

string logPath = "logs/api-log-.txt";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        theme: SystemConsoleTheme.Colored,
        restrictedToMinimumLevel: LogEventLevel.Information
    )
    .WriteTo.File(
        logPath,
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Warning,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        fileSizeLimitBytes: 1_000_000,
        rollOnFileSizeLimit: true,
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    Log.Information("Starting API application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseSerilog();

    builder.Services.AddControllers()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });

    builder.Services.AddOpenApi();

    string? connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        Log.Fatal("Database connection string 'DEFAULT_CONNECTION' is missing or empty.");
        throw new InvalidOperationException("Database connection string 'DEFAULT_CONNECTION' is required.");
    }

    builder.Services.AddDbContext<QuestionaireDbContext>(options =>
    {
        options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        options.UseSqlServer(connectionString,
            sqlOptions =>
            {
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null
                );
            });
    });

    builder.Services.AddAuthorization();
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.BearerScheme;
            options.DefaultSignInScheme = IdentityConstants.BearerScheme;
            options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
        })
        .AddBearerToken(IdentityConstants.BearerScheme);

    builder.Services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<QuestionaireDbContext>()
        .AddApiEndpoints();

    builder.Services.AddScoped<IAnswerService, AnswerService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IQuestionService, QuestionService>();
    builder.Services.AddScoped<IUserQuestionHistoryService, UserQuestionHistoryService>();
    builder.Services.AddScoped<IQuestionCategoriesService, QuestionCategoriesService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IRoleService, RoleService>();
    builder.Services.AddScoped<ILogService, LogService>();

    string? applicationUrl = Environment.GetEnvironmentVariable("API_URL");
    string? webUrl = Environment.GetEnvironmentVariable("WEB_URL");

    if (string.IsNullOrEmpty(webUrl))
        Log.Warning("WEB_URL environment variable not set. CORS might not allow Blazor client.");

    var allowedOrigins = new List<string>();
    if (!string.IsNullOrEmpty(applicationUrl)) allowedOrigins.Add(applicationUrl.TrimEnd('/'));
    if (!string.IsNullOrEmpty(webUrl)) allowedOrigins.Add(webUrl.TrimEnd('/'));

    if (allowedOrigins.Any())
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Cors", policy =>
            {
                policy.WithOrigins(allowedOrigins.ToArray())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }
    else
    {
        Log.Warning("No valid origins specified for CORS policy 'Cors'. Requests might be blocked.");
    }
    
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        
        options.AddFixedWindowLimiter(policyName: "fixed", fixedWindowOptions =>
        {
            fixedWindowOptions.PermitLimit = 30;
            fixedWindowOptions.Window = TimeSpan.FromMinutes(1);
            fixedWindowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            fixedWindowOptions.QueueLimit = 2;
        });
    });

    WebApplication app = builder.Build();

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            Log.Error(exceptionHandlerPathFeature?.Error, "Unhandled exception occurred at path {Path}",
                exceptionHandlerPathFeature?.Path);

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            object errorResponse = app.Environment.IsDevelopment()
                ? new
                {
                    message = "An application error has occurred.", detail = exceptionHandlerPathFeature?.Error.Message
                }
                : new { message = "An application error has occurred. Please try again later." };
            await context.Response.WriteAsJsonAsync(errorResponse);
        });
    });

    if (app.Environment.IsDevelopment())
    {
        app.MapScalarApiReference();
        app.MapOpenApi();
        Log.Information("Development environment detected. OpenAPI and Scalar enabled.");
    }
    else
    {
        app.UseHsts();
    }

    try
    {
        Log.Information("Applying database migrations...");
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<QuestionaireDbContext>();
            dbContext.Database.Migrate();
        }

        Log.Information("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while applying database migrations.");
    }

    app.CustomMapIdentityApi<User>();

    app.UseHttpsRedirection();

    if (allowedOrigins.Any())
    {
        app.UseCors("Cors");
        Log.Information("CORS policy 'Cors' enabled for origins: {Origins}", string.Join(", ", allowedOrigins));
    }

    app.UseRouting();
    
    app.UseRateLimiter();
    Log.Information("Rate Limiting middleware enabled.");
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers()
        .RequireRateLimiting("fixed");

    Log.Information("Application configuration complete. Starting...");
    app.Run();
    Log.Information("Application Started.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Host terminated unexpectedly during startup.");
}
finally
{
    Log.CloseAndFlush();
}