namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Request para atualizar o status de um usuário.
/// Conforme user-status-plan.md.
/// </summary>
public record UpdateUserStatusRequest
{
    /// <summary>
    /// Novo status do usuário.
    /// Valores válidos: "Active", "Suspended", "Rejected".
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Motivo obrigatório para a mudança de status.
    /// </summary>
    public required string Reason { get; init; }
}
