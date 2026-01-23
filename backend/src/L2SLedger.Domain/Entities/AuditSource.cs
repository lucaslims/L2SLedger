namespace L2SLedger.Domain.Entities;

/// <summary>
/// Origem da operação auditada.
/// </summary>
public enum AuditSource
{
    /// <summary>Interface do usuário (frontend)</summary>
    UI = 1,

    /// <summary>API REST direta</summary>
    API = 2,

    /// <summary>Importação de dados</summary>
    Import = 3,

    /// <summary>Job em background</summary>
    BackgroundJob = 4,

    /// <summary>Sistema (automático)</summary>
    System = 5
}
