using Serilog;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de observabilidade (Serilog, logs estruturados).
/// Conforme ADR-006 (Observabilidade), ADR-013 (LGPD).
/// </summary>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Configura Serilog com logs estruturados (Console + Arquivo).
    /// </summary>
    public static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
            .WriteTo.File(
                formatter: new Serilog.Formatting.Compact.CompactJsonFormatter(),
                path: "logs/l2sledger-.log",
                rollingInterval: RollingInterval.Minute,
                flushToDiskInterval: TimeSpan.FromDays(5),
                retainedFileCountLimit: 10)
            .Enrich.WithProperty("Application", "L2SLedger.API")
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            .Enrich.FromLogContext()
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
    /// Configura Serilog request logging.
    /// </summary>
    public static WebApplication UseSerilogConfiguration(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        return app;
    }
}
