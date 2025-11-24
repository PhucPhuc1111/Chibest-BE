using Chibest.Repository.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chibest.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(
        this WebApplication app,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChiBestDbContext>();

        logger.LogInformation("Applying EF Core migrations...");
        await dbContext.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("EF Core migrations completed successfully.");
    }

    public static bool ShouldAutoApplyMigrations(this IConfiguration configuration)
    {
        if (bool.TryParse(Environment.GetEnvironmentVariable("AUTO_APPLY_MIGRATIONS"), out var envFlag))
        {
            return envFlag;
        }

        return configuration.GetValue("Database:AutoMigrate", false);
    }
}

