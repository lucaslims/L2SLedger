namespace L2SLedger.Domain.Exceptions;

/// <summary>
/// Exceção para violações de regras de negócio e validações de domínio.
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string code, string message)
        : base(code, message)
    {
    }

    public BusinessRuleException(string code, string message, Exception innerException)
        : base(code, message, innerException)
    {
    }
}
