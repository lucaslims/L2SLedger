using L2SLedger.API.Contracts;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace L2SLedger.API.Middleware;

/// <summary>
/// Middleware global para capturar exceções e retornar respostas JSON padronizadas.
/// Conforme ADR-021 (modelo de erros semântico).
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exceção não tratada capturada pelo GlobalExceptionHandler");

        var (statusCode, errorCode, message) = exception switch
        {
            AuthenticationException authEx => (HttpStatusCode.Unauthorized, authEx.Code, authEx.Message),
            BusinessRuleException businessEx => (HttpStatusCode.BadRequest, businessEx.Code, businessEx.Message),
            FluentValidation.ValidationException => (HttpStatusCode.BadRequest, "VAL_VALIDATION_FAILED", "Falha na validação dos dados"),
            _ => (HttpStatusCode.InternalServerError, "SYS_INTERNAL_ERROR", "Erro interno do servidor")
        };

        var errorResponse = ErrorResponse.Create(
            errorCode,
            message,
            httpContext.TraceIdentifier
        );

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true;
    }
}
