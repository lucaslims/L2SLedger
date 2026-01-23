namespace L2SLedger.Domain.Entities;

/// <summary>
/// Tipos de eventos auditáveis conforme ADR-014.
/// </summary>
public enum AuditEventType
{
    /// <summary>Criação de entidade</summary>
    Create = 1,

    /// <summary>Atualização de entidade</summary>
    Update = 2,

    /// <summary>Exclusão (soft delete)</summary>
    Delete = 3,

    /// <summary>Importação de dados</summary>
    Import = 4,

    /// <summary>Ajuste pós-fechamento</summary>
    Adjust = 5,

    /// <summary>Fechamento de período</summary>
    Close = 6,

    /// <summary>Reabertura de período</summary>
    Reopen = 7,

    /// <summary>Login bem-sucedido</summary>
    Login = 10,

    /// <summary>Logout</summary>
    Logout = 11,

    /// <summary>Tentativa de login falha</summary>
    LoginFailed = 12,

    /// <summary>Acesso negado (403)</summary>
    AccessDenied = 13,

    /// <summary>Exportação de dados</summary>
    Export = 20
}
