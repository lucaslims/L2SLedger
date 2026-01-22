using L2SLedger.API.Middleware;
using Serilog;
using Serilog.Events;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de observabilidade (Serilog, logs estruturados).
/// Conforme ADR-006 (Observabilidade), ADR-013 (LGPD).
/// </summary>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Configura Serilog com logs estruturados (Console + Arquivo).
    /// Atualizado para incluir CorrelationId.
    /// </summary>
    public static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
            .WriteTo.File(
                formatter: new Serilog.Formatting.Compact.CompactJsonFormatter(),
                path: "logs/l2sledger-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 50_000_000) // 50MB por arquivo
            .Enrich.WithProperty("Application", "L2SLedger.API")
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown")
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .Enrich.FromLogContext() // Importante para CorrelationId
            .CreateLogger();
    }

    /// <summary>
    /// Adiciona Serilog como logger principal.
    /// </summary>
    public static WebApplicationBuilder AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog();
        return builder;
    }

    /// <summary>
    /// Configura middleware de observabilidade.
    /// </summary>
    public static WebApplication UseObservabilityConfiguration(this WebApplication app)
    {
        // Correlation ID primeiro para todas as requisições
        app.UseCorrelationId();

        // Serilog request logging
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                // Adicionar informações extras ao log de requisição
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    var userId = httpContext.User.FindFirst("sub")?.Value;
                    if (userId is not null)
                    {
                        diagnosticContext.Set("UserId", userId);
                    }
                }
            };
        });

        return app;
    }

    /// <summary>
    /// Configura Serilog request logging.
    /// </summary>
    [Obsolete("Use UseObservabilityConfiguration() que inclui Correlation ID. Mantido para retrocompatibilidade.")]
    public static WebApplication UseSerilogConfiguration(this WebApplication app)
    {
        return app.UseObservabilityConfiguration();
    }
}
