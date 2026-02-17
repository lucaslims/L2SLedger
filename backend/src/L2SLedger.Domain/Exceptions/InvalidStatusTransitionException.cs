using L2SLedger.Domain.Constants;

namespace L2SLedger.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma transição de status de usuário é inválida.
/// </summary>
public class InvalidStatusTransitionException : DomainException
{
    public InvalidStatusTransitionException(string currentStatus, string newStatus)
        : base(ErrorCodes.USER_INVALID_STATUS_TRANSITION,
               $"Não é possível alterar o status de {currentStatus} para {newStatus}.")
    {
    }

    public InvalidStatusTransitionException(string message)
        : base(ErrorCodes.USER_INVALID_STATUS_TRANSITION, message)
    {
    }
}
