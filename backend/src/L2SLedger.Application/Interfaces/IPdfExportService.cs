namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Interface do serviço para exportação de dados em formato PDF.
/// </summary>
public interface IPdfExportService
{
    /// <summary>
    /// Exporta transações para um arquivo PDF com base nos filtros fornecidos.
    /// </summary>
    /// <param name="userId">ID do usuário cujas transações serão exportadas.</param>
    /// <param name="startDate">Data de início para filtrar as transações (opcional).</param>
    /// <param name="endDate">Data de término para filtrar as transações (opcional).</param>
    /// <param name="categoryId">ID da categoria para filtrar as transações (opcional).</param>
    /// <param name="transactionType">Tipo da transação para filtrar (opcional).</param>
    /// <returns>Tupla contendo o caminho do arquivo PDF gerado e o número de registros exportados.</returns>
    Task<(string FilePath, int RecordCount)> ExportTransactionsToPdfAsync(
        Guid userId,
        DateTime? startDate,
        DateTime? endDate,
        Guid? categoryId,
        int? transactionType);
}
