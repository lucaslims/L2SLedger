using L2SLedger.Infrastructure.Observability;
using OpenTelemetry.Metrics;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de métricas OpenTelemetry para a API.
/// Conforme ADR-006: Métricas expostas via /metrics para Prometheus.
/// </summary>
public static class MetricsExtensions
{
    /// <summary>
    /// Adiciona configuração de métricas OpenTelemetry.
    /// </summary>
    public static IServiceCollection AddMetricsConfiguration(this IServiceCollection services)
    {
        // Registrar métricas customizadas
        services.AddSingleton<ApplicationMetrics>();

        // Configurar OpenTelemetry
        services.AddOpenTelemetry()
            .WithMetrics(options =>
            {
                // Instrumentação padrão do ASP.NET Core
                options.AddAspNetCoreInstrumentation();

                // Instrumentação de runtime (.NET)
                options.AddRuntimeInstrumentation();

                // Métricas customizadas da aplicação
                options.AddMeter(ApplicationMetrics.MeterName);

                // Expor métricas para Prometheus
                options.AddPrometheusExporter();
            });

        return services;
    }

    /// <summary>
    /// Mapeia endpoint de métricas Prometheus.
    /// </summary>
    public static WebApplication MapMetricsEndpoint(this WebApplication app)
    {
        // /metrics - Endpoint para Prometheus scraping
        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        return app;
    }
}
