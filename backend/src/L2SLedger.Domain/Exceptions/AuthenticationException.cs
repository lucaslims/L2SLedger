namespace L2SLedger.Domain.Exceptions;

/// <summary>
/// Exceção para erros de autenticação e autorização.
/// Conforme ADR-021 (modelo de erros semânticos).
/// </summary>
public class AuthenticationException : DomainException
{
    public AuthenticationException(string code, string message)
        : base(code, message)
    {
    }

    public AuthenticationException(string code, string message, Exception innerException)
        : base(code, message, innerException)
    {
    }
}
