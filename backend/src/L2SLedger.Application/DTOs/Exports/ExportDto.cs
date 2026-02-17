namespace L2SLedger.Application.DTOs.Exports;

/// <summary>
/// DTO para representar uma exportação.
/// </summary>
public class ExportDto
{
    /// <summary>
    /// ID da exportação.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tipo de exportação (e.g., Transactions, Reports).
    /// </summary>
    public string ExportType { get; set; } = string.Empty;

    /// <summary>
    /// Formato do arquivo exportado (e.g., CSV, PDF).
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Status da exportação.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Caminho do arquivo exportado, se disponível.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Tamanho do arquivo em bytes, se disponível.
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Parâmetros usados para gerar a exportação, em formato JSON.
    /// </summary>
    public string ParametersJson { get; set; } = "{}";

    /// <summary>
    /// ID do usuário que solicitou a exportação.
    /// </summary>
    public Guid RequestedByUserId { get; set; }

    /// <summary>
    /// Nome do usuário que solicitou a exportação.
    /// </summary>
    public string? RequestedByUserName { get; set; }

    /// <summary>
    /// Data e hora da solicitação.
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// Data e hora do início do processamento, se aplicável.
    /// </summary>
    public DateTime? ProcessingStartedAt { get; set; }

    /// <summary>
    /// Data e hora da conclusão, se aplicável.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Mensagem de erro, se houver.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Número de registros exportados, se aplicável.
    /// </summary>
    public int? RecordCount { get; set; }

    /// <summary>
    /// Data e hora da criação do registro de exportação.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
