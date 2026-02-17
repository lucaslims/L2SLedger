using L2SLedger.API.Contracts;
using L2SLedger.Domain.Exceptions;
using L2SLedger.Domain.Constants;
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
            NotFoundException notFoundEx => (HttpStatusCode.NotFound, notFoundEx.Code, notFoundEx.Message),
            AuthorizationException authzEx => (HttpStatusCode.Forbidden, authzEx.Code, authzEx.Message),
            AuthenticationException authEx => (HttpStatusCode.Unauthorized, authEx.Code, authEx.Message),
            BusinessRuleException businessEx => (HttpStatusCode.BadRequest, businessEx.Code, businessEx.Message),
            FluentValidation.ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ErrorCodes.VAL_VALIDATION_FAILED,
                string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),
            _ => (HttpStatusCode.InternalServerError, ErrorCodes.SYS_INTERNAL_ERROR, "Erro interno do servidor")
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
