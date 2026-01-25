using System.Globalization;
using System.Text;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.Services;

/// <summary>
/// Serviço para exportar transações em formato CSV.
/// </summary>
public class CsvExportService : ICsvExportService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<CsvExportService> _logger;

    public CsvExportService(
        ITransactionRepository transactionRepository,
        IFileStorageService fileStorageService,
        ILogger<CsvExportService> logger)
    {
        _transactionRepository = transactionRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<(string FilePath, int RecordCount)> ExportTransactionsToCsvAsync(
        Guid userId,
        DateTime? startDate,
        DateTime? endDate,
        Guid? categoryId,
        int? transactionType)
    {
        // Query transações
        var (transactions, totalCount) = await _transactionRepository.GetByFiltersAsync(
            userId: userId,
            categoryId: categoryId,
            type: transactionType.HasValue ? (TransactionType)transactionType.Value : null,
            startDate: startDate,
            endDate: endDate,
            page: 1,
            pageSize: int.MaxValue // Exportar todas
        );

        // Gerar CSV
        var csv = new StringBuilder();
        csv.AppendLine("Date,Description,Category,Amount,Type");

        foreach (var transaction in transactions)
        {
            var date = transaction.TransactionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var description = EscapeCsvField(transaction.Description);
            var category = transaction.Category != null ? EscapeCsvField(transaction.Category.Name) : "";
            var amount = transaction.Amount.ToString("F2", CultureInfo.InvariantCulture);
            var type = transaction.Type == TransactionType.Income ? "Income" : "Expense";

            csv.AppendLine($"{date},{description},{category},{amount},{type}");
        }

        // Incluir userId completo e componentes adicionais para evitar conflitos entre exportações simultâneas
        var userIdFull = userId.ToString("N");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"transactions_{userIdFull}_{timestamp}_{uniqueSuffix}.csv";

        var fileBytes = Encoding.UTF8.GetBytes(csv.ToString());
        var filePath = await _fileStorageService.SaveExportFileAsync(fileBytes, fileName);

        _logger.LogInformation(
            "CSV export generated: {FilePath}, Records: {Count}",
            filePath,
            transactions.Count
        );

        return (filePath, transactions.Count);
    }

    private string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
