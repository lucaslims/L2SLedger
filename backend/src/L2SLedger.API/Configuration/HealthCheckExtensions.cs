using L2SLedger.Infrastructure.HealthChecks;
using L2SLedger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de Health Checks para a API.
/// Conforme ADR-006: Health checks para PostgreSQL e Firebase.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adiciona Health Checks ao container de DI.
    /// </summary>
    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services)
    {
        services.AddHealthChecks()
            // Health Check do PostgreSQL via EF Core
            .AddDbContextCheck<L2SLedgerDbContext>(
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db"])
            // Health Check customizado do Firebase
            .AddCheck<FirebaseHealthCheck>(
                name: "firebase",
                failureStatus: HealthStatus.Degraded,
                tags: ["ready", "external"]);

        // Registrar HttpClient para FirebaseHealthCheck
        services.AddHttpClient<FirebaseHealthCheck>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }

    /// <summary>
    /// Mapeia endpoints de Health Checks.
    /// </summary>
    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // /health - Health check básico (apenas aplicação)
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false, // Não executa nenhum check específico
            ResponseWriter = WriteHealthResponse
        });

        // /health/ready - Readiness probe (PostgreSQL + Firebase)
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthResponse
        });

        // /health/live - Liveness probe (apenas se a aplicação está rodando)
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false, // Liveness não executa checks
            ResponseWriter = WriteHealthResponse
        });

        return app;
    }

    /// <summary>
    /// Escreve resposta JSON detalhada para health checks.
    /// </summary>
    private static async Task WriteHealthResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                exception = entry.Value.Exception?.Message,
                data = entry.Value.Data
            })
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, options));
    }
}
