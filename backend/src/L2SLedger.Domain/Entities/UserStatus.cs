namespace L2SLedger.Domain.Entities;

/// <summary>
/// Status de aprovação do usuário no sistema.
/// Conforme planejamento de integração Frontend-Backend (user-status-plan.md).
/// </summary>
public enum UserStatus
{
    /// <summary>Aguardando aprovação do Admin.</summary>
    Pending = 0,
    
    /// <summary>Aprovado e pode acessar o sistema.</summary>
    Active = 1,
    
    /// <summary>Suspenso temporariamente.</summary>
    Suspended = 2,
    
    /// <summary>Cadastro rejeitado.</summary>
    Rejected = 3
}
