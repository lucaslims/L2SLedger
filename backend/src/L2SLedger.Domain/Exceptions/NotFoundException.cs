namespace L2SLedger.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando um recurso não é encontrado.
/// Conforme ADR-021 (Modelo de Erros).
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string code, string message)
        : base(code, message)
    {
    }

    public NotFoundException(string code, string message, Exception innerException)
        : base(code, message, innerException)
    {
    }
}
