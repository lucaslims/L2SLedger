namespace L2SLedger.Application.DTOs.Exports;

/// <summary>
/// Response com status de uma exportação.
/// </summary>
public class ExportStatusResponse
{
    /// <summary>
    /// ID da exportação.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Status da exportação.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem de erro, se houver.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Porcentagem de progresso (0-100), se aplicável.
    /// </summary>
    public int? ProgressPercentage { get; set; }

    /// <summary>
    /// Data e hora da solicitação.
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// Data e hora da conclusão, se aplicável.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Indica se o arquivo está disponível para download.
    /// </summary>
    public bool IsDownloadable { get; set; }
}
