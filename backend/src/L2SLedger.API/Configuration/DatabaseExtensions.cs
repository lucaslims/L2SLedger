using L2SLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de banco de dados e Entity Framework Core.
/// Conforme ADR-034 (PostgreSQL), ADR-035 (Migrations).
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Configura o DbContext com PostgreSQL e migrations.
    /// </summary>
    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddDbContext<L2SLedgerDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("L2SLedger.Infrastructure");
            });

            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }

    /// <summary>
    /// Aplica migrations automaticamente em ambiente de desenvolvimento.
    /// </summary>
    public static async Task<WebApplication> ApplyMigrationsAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<L2SLedgerDbContext>();
            await dbContext.Database.MigrateAsync();
            Log.Information("Migrations aplicadas com sucesso");
        }

        return app;
    }
}
