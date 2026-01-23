using Serilog.Context;

namespace L2SLedger.API.Middleware;

/// <summary>
/// Middleware para gerenciar Correlation ID em requisições.
/// Conforme ADR-006: Correlação de requisições via Correlation ID.
/// 
/// NOTA: Este middleware fica na camada API pois depende de HttpContext,
/// que é um conceito da camada de apresentação (Clean Architecture).
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug("Request iniciado com CorrelationId: {CorrelationId}", correlationId);
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingId)
            && !string.IsNullOrWhiteSpace(existingId))
        {
            return existingId.ToString();
        }
        return Guid.NewGuid().ToString("N")[..16];
    }
}

/// <summary>
/// Extension methods para registrar o middleware.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
