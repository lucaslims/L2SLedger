namespace L2SLedger.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando usuário não tem permissão para acessar um recurso.
/// Conforme ADR-021 (Modelo de Erros) e ADR-016 (RBAC).
/// </summary>
public class AuthorizationException : DomainException
{
    public AuthorizationException(string code, string message)
        : base(code, message)
    {
    }

    public AuthorizationException(string code, string message, Exception innerException)
        : base(code, message, innerException)
    {
    }
}
