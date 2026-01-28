using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Domain.Entities;

/// <summary>
/// Representa uma solicitação de exportação de dados.
/// </summary>
public class Export : Entity
{
    /// <summary>
    /// Tipo de dados exportados (ex: "Transactions", "CashFlowReport").
    /// </summary>
    public string ExportType { get; private set; } = string.Empty;

    /// <summary>
    /// Formato da exportação (CSV, PDF).
    /// </summary>
    public ExportFormat Format { get; private set; }

    /// <summary>
    /// Status atual da exportação.
    /// </summary>
    public ExportStatus Status { get; private set; }

    /// <summary>
    /// Caminho do arquivo gerado (relativo ou absoluto).
    /// </summary>
    public string? FilePath { get; private set; }

    /// <summary>
    /// Tamanho do arquivo em bytes (após geração).
    /// </summary>
    public long? FileSizeBytes { get; private set; }

    /// <summary>
    /// Parâmetros da exportação em JSON (filtros, período, etc).
    /// </summary>
    public string ParametersJson { get; private set; } = "{}";

    /// <summary>
    /// ID do usuário que solicitou a exportação.
    /// </summary>
    public Guid RequestedByUserId { get; private set; }

    /// <summary>
    /// Data/hora da solicitação.
    /// </summary>
    public DateTime RequestedAt { get; private set; }

    /// <summary>
    /// Data/hora do início do processamento.
    /// </summary>
    public DateTime? ProcessingStartedAt { get; private set; }

    /// <summary>
    /// Data/hora da conclusão (sucesso ou falha).
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Mensagem de erro (se Status = Failed).
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Número de registros exportados.
    /// </summary>
    public int? RecordCount { get; private set; }

    // Navigation property
    public User? RequestedByUser { get; set; }

    // Constructor for EF Core
    private Export() { }

    /// <summary>
    /// Cria nova solicitação de exportação.
    /// </summary>
    public Export(
        string exportType,
        ExportFormat format,
        string parametersJson,
        Guid requestedByUserId)
    {
        ExportType = exportType;
        Format = format;
        Status = ExportStatus.Pending;
        ParametersJson = parametersJson;
        RequestedByUserId = requestedByUserId;
        RequestedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca exportação como iniciada.
    /// </summary>
    public void MarkAsProcessing()
    {
        if (Status != ExportStatus.Pending)
            throw new BusinessRuleException(ErrorCodes.EXPORT_INVALID_STATE, "Only pending exports can be marked as processing.");

        Status = ExportStatus.Processing;
        ProcessingStartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca exportação como concluída.
    /// </summary>
    public void MarkAsCompleted(string filePath, long fileSizeBytes, int recordCount)
    {
        if (Status != ExportStatus.Processing)
            throw new BusinessRuleException(ErrorCodes.EXPORT_INVALID_STATE, "Only processing exports can be marked as completed.");

        Status = ExportStatus.Completed;
        FilePath = filePath;
        FileSizeBytes = fileSizeBytes;
        RecordCount = recordCount;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca exportação como falha.
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        if (Status != ExportStatus.Processing)
            throw new BusinessRuleException(ErrorCodes.EXPORT_INVALID_STATE, "Only processing exports can be marked as failed.");

        Status = ExportStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se a exportação pode ser baixada.
    /// </summary>
    public bool IsDownloadable() => Status == ExportStatus.Completed && !string.IsNullOrEmpty(FilePath);
}
