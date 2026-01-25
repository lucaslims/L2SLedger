using System.Text;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.Services;

/// <summary>
/// Serviço para exportar transações em formato PDF.
/// </summary>
public class PdfExportService : IPdfExportService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PdfExportService> _logger;

    public PdfExportService(
        ITransactionRepository transactionRepository,
        IFileStorageService fileStorageService,
        ILogger<PdfExportService> logger)
    {
        _transactionRepository = transactionRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<(string FilePath, int RecordCount)> ExportTransactionsToPdfAsync(
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
            pageSize: int.MaxValue
        );

        // Gerar PDF simples (HTML-to-PDF básico)
        // Nota: Em produção, usar biblioteca como QuestPDF ou iTextSharp
        var html = GenerateHtmlReport(transactions, startDate, endDate);
        
        
        // Incluir userId completo e componentes adicionais para evitar conflitos entre exportações simultâneas
        var userIdFull = userId.ToString("N");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"transactions_{userIdFull}_{timestamp}_{uniqueSuffix}.html";

        var fileBytes = Encoding.UTF8.GetBytes(html);

        // Por ora, salvar como HTML (substituir por geração de PDF real em produção)
        var filePath = await _fileStorageService.SaveExportFileAsync(fileBytes, fileName);

        _logger.LogInformation(
            "PDF export generated: {FilePath}, Records: {Count}",
            filePath,
            transactions.Count
        );

        return (filePath, transactions.Count);
    }

    private string GenerateHtmlReport(List<Transaction> transactions, DateTime? startDate, DateTime? endDate)
    {
        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var netBalance = totalIncome - totalExpense;

        var periodText = startDate.HasValue && endDate.HasValue
            ? $"{startDate.Value:yyyy-MM-dd} to {endDate.Value:yyyy-MM-dd}"
            : "All time";

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head><meta charset='utf-8'><title>Transaction Report</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine("h1 { color: #333; }");
        html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
        html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.AppendLine("th { background-color: #4CAF50; color: white; }");
        html.AppendLine(".summary { margin: 20px 0; padding: 10px; background: #f9f9f9; }");
        html.AppendLine(".income { color: green; }");
        html.AppendLine(".expense { color: red; }");
        html.AppendLine("</style></head><body>");
        
        html.AppendLine("<h1>L2SLedger - Transaction Report</h1>");
        html.AppendLine($"<p><strong>Period:</strong> {periodText}</p>");
        
        html.AppendLine("<div class='summary'>");
        html.AppendLine($"<p><strong>Total Income:</strong> <span class='income'>R$ {totalIncome:N2}</span></p>");
        html.AppendLine($"<p><strong>Total Expense:</strong> <span class='expense'>R$ {totalExpense:N2}</span></p>");
        html.AppendLine($"<p><strong>Net Balance:</strong> R$ {netBalance:N2}</p>");
        html.AppendLine($"<p><strong>Total Transactions:</strong> {transactions.Count}</p>");
        html.AppendLine("</div>");
        
        html.AppendLine("<table>");
        html.AppendLine("<thead><tr><th>Date</th><th>Description</th><th>Category</th><th>Amount</th><th>Type</th></tr></thead>");
        html.AppendLine("<tbody>");
        
        foreach (var transaction in transactions.OrderBy(t => t.TransactionDate))
        {
            var cssClass = transaction.Type == TransactionType.Income ? "income" : "expense";
            html.AppendLine("<tr>");
            html.AppendLine($"<td>{transaction.TransactionDate:yyyy-MM-dd}</td>");
            html.AppendLine($"<td>{transaction.Description}</td>");
            html.AppendLine($"<td>{transaction.Category?.Name ?? ""}</td>");
            html.AppendLine($"<td class='{cssClass}'>R$ {transaction.Amount:N2}</td>");
            html.AppendLine($"<td>{transaction.Type}</td>");
            html.AppendLine("</tr>");
        }
        
        html.AppendLine("</tbody></table>");
        html.AppendLine("</body></html>");
        
        return html.ToString();
    }
}
