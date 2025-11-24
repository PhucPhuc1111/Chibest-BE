using Chibest.API.Extensions;
using Chibest.API.Health;
using Chibest.API.Middleware;
using Chibest.Repository.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
var isRunningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);
if (!isRunningInContainer && File.Exists(envFile))
{
    DotNetEnv.Env.Load(envFile);
}

builder.Configuration.AddJsonFile("excel-mappings.json", optional: true, reloadOnChange: true);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddJsonConsole();

var databaseConnectionString = Environment.GetEnvironmentVariable("DB_PG_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string is not configured. Set DB_PG_CONNECTION_STRING or ConnectionStrings:DefaultConnection.");

var autoApplyMigrations = builder.Configuration.ShouldAutoApplyMigrations();
var migrateOnly = args.Any(a => string.Equals(a, "--migrate", StringComparison.OrdinalIgnoreCase));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Services.AddMemoryCache();

ServiceRegister.RegisterServices(builder.Services, builder.Configuration, databaseConnectionString);

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddCheck<DatabaseHealthCheck>(
        "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready" });

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (migrateOnly || autoApplyMigrations)
{
    await app.ApplyDatabaseMigrationsAsync(app.Logger);
    if (migrateOnly)
    {
        return;
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseCors("FrontendCors");

app.Use(async (context, next) =>
{
    if (HttpMethods.IsOptions(context.Request.Method))
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        return;
    }
    await next();
});

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpMetrics();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapMetrics("/metrics");
app.MapControllers();

await app.RunAsync();