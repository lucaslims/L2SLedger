namespace L2SLedger.Domain.Exceptions;

/// <summary>
/// Exceção base para todas as exceções de domínio.
/// Utilizada para representar violações de regras de negócio.
/// </summary>
public abstract class DomainException : Exception
{
    public string Code { get; }

    protected DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    protected DomainException(string code, string message, Exception innerException) 
        : base(message, innerException)
    {
        Code = code;
    }
}
