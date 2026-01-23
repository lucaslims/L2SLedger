namespace L2SLedger.Domain.Entities;

/// <summary>
/// Formato de exportação suportado.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// Comma-Separated Values (CSV).
    /// </summary>
    Csv = 1,

    /// <summary>
    /// Portable Document Format (PDF).
    /// </summary>
    Pdf = 2
}
