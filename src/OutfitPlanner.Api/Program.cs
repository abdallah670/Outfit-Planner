using OutfitPlanner.Api.Middleware;
using OutfitPlanner.Api.Converters;
using OutfitPlanner.Infrastructure;
using OutfitPlanner.Persistence.Data;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using OutfitPlanner.Application;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.SqlServer;
using OutfitPlanner.Infrastructure.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/outfitplanner-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableTimeSpanConverter());
    });
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Outfit Planner API", Version = "v1" });
    
    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.WithOrigins(
            "http://localhost:4200", 
            "https://localhost:4200",
            "http://localhost:5000",
            "https://localhost:5001",
            "http://localhost:5001",
            "https://localhost:5000")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .SetIsOriginAllowed(_ => true)); // Allow any origin for development
});

// Add Infrastructure Services
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application Services
builder.Services.AddApplication(builder.Configuration);

// Add Memory Cache for search results
builder.Services.AddMemoryCache();

// Add Hangfire for background job processing
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add Hangfire server
builder.Services.AddHangfireServer();

// Register Hangfire job
builder.Services.AddTransient<TrendingCalculationHangfireJob>();

// Identity.Application cookie scheme used automatically by AddIdentity for external login callbacks

// Add OAuth providers (chains to existing JWT auth configured in AddPersistence)
var googleClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"] ?? "";
var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? "";

// Check if OAuth credentials are configured (not empty and not placeholders)
bool hasGoogleOAuth = !string.IsNullOrWhiteSpace(googleClientId) && 
                      !string.IsNullOrWhiteSpace(googleClientSecret) &&
                      !googleClientId.Contains("YOUR_") &&
                      !googleClientId.Contains("example");

bool hasFacebookOAuth = !string.IsNullOrWhiteSpace(facebookAppId) && 
                        !string.IsNullOrWhiteSpace(facebookAppSecret) &&
                        !facebookAppId.Contains("YOUR_") &&
                        !facebookAppId.Contains("example");

// Build authentication chain
var authBuilder = builder.Services.AddAuthentication();

// Identity.External cookie scheme provided automatically by AddIdentity
// OAuth providers will chain to it via SignInScheme = "Identity.External"

// Add Google OAuth if credentials exist
if (hasGoogleOAuth)
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/signin-google";
        options.SignInScheme = "Identity.External";
        options.SaveTokens = true;
    });
    Log.Information("Google OAuth configured");
}

// Add Facebook OAuth if credentials exist
if (hasFacebookOAuth)
{
    authBuilder.AddFacebook(options =>
    {
        options.AppId = facebookAppId;
        options.AppSecret = facebookAppSecret;
        options.CallbackPath = "/signin-facebook";
        options.SignInScheme = "Identity.External";
        options.SaveTokens = true;
    });
    Log.Information("Facebook OAuth configured");
}

if (!hasGoogleOAuth && !hasFacebookOAuth)
{
    Log.Warning("OAuth credentials not found. Social login buttons will fail if clicked.");
}

var app = builder.Build();

// Add Request Logging Middleware
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

// Apply CORS before HTTPS redirection to handle preflight requests properly
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Outfit Planner API v1");
        options.RoutePrefix = "swagger";
    });
}

// Ensure wwwroot/uploads exists
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard - accessible at /hangfire
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Outfit Planner Jobs",
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Redirect to Swagger UI instead of Scalar for free documentation
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/api", () => Results.Redirect("/swagger"));

app.MapControllers();

try 
{
    Log.Information("Starting web host");
    
    // Seed the database with initial data
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }

    // Schedule recurring Hangfire job for daily trending calculation
    using (var scope = app.Services.CreateScope())
    {
        var job = scope.ServiceProvider.GetRequiredService<TrendingCalculationHangfireJob>();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        
        // Schedule job to run daily at midnight UTC
        recurringJobManager.AddOrUpdate(
            "daily-trending-calculation",
            () => job.CalculateTrendingAsync(),
            "0 0 * * *", // Cron: At 00:00 every day
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            }
        );
        
        Log.Information("Hangfire recurring job 'daily-trending-calculation' scheduled to run daily at midnight UTC");
    }

    // Schedule recurring Hangfire job for auto-unlocking accounts
    using (var scope = app.Services.CreateScope())
    {
        var unlockJob = scope.ServiceProvider.GetRequiredService<AccountUnlockBackgroundJob>();
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        
        // Schedule job to run every 5 minutes
        recurringJobManager.AddOrUpdate(
            "auto-unlock-accounts",
            () => unlockJob.AutoUnlockAccounts(),
            "*/5 * * * *", // Cron: Every 5 minutes
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            }
        );
        
        Log.Information("Hangfire recurring job 'auto-unlock-accounts' scheduled to run every 5 minutes");
    }
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration testing
public partial class Program { }
