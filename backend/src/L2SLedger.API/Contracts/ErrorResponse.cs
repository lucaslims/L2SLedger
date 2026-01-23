namespace L2SLedger.API.Contracts;

/// <summary>
/// Contrato padrão de resposta de erro da API.
/// Conforme ADR-021 - Modelo de Erros Semântico e Fail-Fast.
/// </summary>
public record ErrorResponse
{
    public required ErrorDetail Error { get; init; }

    public record ErrorDetail
    {
        /// <summary>
        /// Código semântico do erro (ex: AUTH_INVALID_TOKEN, FIN_PERIOD_CLOSED).
        /// </summary>
        public required string Code { get; init; }

        /// <summary>
        /// Mensagem descritiva do erro.
        /// </summary>
        public required string Message { get; init; }

        /// <summary>
        /// Detalhes adicionais sobre o erro (opcional).
        /// </summary>
        public string? Details { get; init; }

        /// <summary>
        /// Timestamp UTC de quando o erro ocorreu.
        /// </summary>
        public required DateTime Timestamp { get; init; }

        /// <summary>
        /// ID de correlação para rastreamento (TraceId).
        /// </summary>
        public required string TraceId { get; init; }
    }

    public static ErrorResponse Create(string code, string message, string? details = null, string? traceId = null)
    {
        return new ErrorResponse
        {
            Error = new ErrorDetail
            {
                Code = code,
                Message = message,
                Details = details,
                Timestamp = DateTime.UtcNow,
                TraceId = traceId ?? Guid.NewGuid().ToString()
            }
        };
    }
}
