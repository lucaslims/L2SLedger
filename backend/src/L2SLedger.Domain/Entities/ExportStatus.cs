namespace L2SLedger.Domain.Entities;

/// <summary>
/// Status de uma exportação.
/// </summary>
public enum ExportStatus
{
    /// <summary>
    /// Exportação pendente de processamento.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Exportação em processamento.
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Exportação concluída com sucesso.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Exportação falhou.
    /// </summary>
    Failed = 4
}
