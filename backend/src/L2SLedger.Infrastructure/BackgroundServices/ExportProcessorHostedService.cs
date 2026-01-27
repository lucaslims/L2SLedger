using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.BackgroundServices;

/// <summary>
/// Background service para processar exportações pendentes.
/// Conforme ADR-017 (Exportação), executa a cada 10 segundos.
/// </summary>
public class ExportProcessorHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExportProcessorHostedService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(10);
    private readonly int _maxConcurrentExports = 5;

    public ExportProcessorHostedService(
        IServiceProvider serviceProvider,
        ILogger<ExportProcessorHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Export Processor Hosted Service iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingExportsAsync(stoppingToken);
                await CleanupOldExportsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar exportações");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Export Processor Hosted Service encerrado");
    }

    private async Task ProcessPendingExportsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var exportRepository = scope.ServiceProvider.GetRequiredService<IExportRepository>();
        var csvService = scope.ServiceProvider.GetRequiredService<ICsvExportService>();
        var pdfService = scope.ServiceProvider.GetRequiredService<IPdfExportService>();

        var pendingExports = await exportRepository.GetPendingAsync(_maxConcurrentExports);

        if (!pendingExports.Any())
            return;

        _logger.LogInformation("Processando {Count} exportações pendentes", pendingExports.Count);

        foreach (var export in pendingExports)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await ProcessExportAsync(export, csvService, pdfService, exportRepository, cancellationToken);
        }
    }

    private async Task ProcessExportAsync(
        Export export,
        ICsvExportService csvService,
        IPdfExportService pdfService,
        IExportRepository exportRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            export.MarkAsProcessing();
            await exportRepository.UpdateAsync(export);

            _logger.LogInformation("Iniciando processamento da exportação {ExportId}", export.Id);

            // Desserializar parâmetros
            var parameters = System.Text.Json.JsonSerializer.Deserialize<ExportParameters>(export.ParametersJson);
            if (parameters == null)
                throw new BusinessRuleException(ErrorCodes.EXPORT_INVALID_PARAMETERS, "Parâmetros da exportação são inválidos");

            (string filePath, int recordCount) result;

            // Processar baseado no formato
            if (export.Format == ExportFormat.Csv)
            {
                result = await csvService.ExportTransactionsToCsvAsync(
                    export.RequestedByUserId,
                    parameters.StartDate,
                    parameters.EndDate,
                    parameters.CategoryId,
                    parameters.TransactionType);
            }
            else // PDF
            {
                result = await pdfService.ExportTransactionsToPdfAsync(
                    export.RequestedByUserId,
                    parameters.StartDate,
                    parameters.EndDate,
                    parameters.CategoryId,
                    parameters.TransactionType);
            }

            // Obter tamanho do arquivo
            var fileStorageService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IFileStorageService>();
            var fileSizeBytes = fileStorageService.GetFileSizeBytes(result.filePath);

            export.MarkAsCompleted(result.filePath, fileSizeBytes, result.recordCount);
            await exportRepository.UpdateAsync(export);

            _logger.LogInformation("Exportação {ExportId} concluída com sucesso - {RecordCount} registros, {SizeBytes} bytes",
                export.Id, result.recordCount, fileSizeBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar exportação {ExportId}", export.Id);

            export.MarkAsFailed(ex.Message);
            await exportRepository.UpdateAsync(export);
        }
    }

    private async Task CleanupOldExportsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        var deletedCount = await fileStorageService.CleanupOldExportsAsync(cutoffDate);

        if (deletedCount > 0)
            _logger.LogInformation("Removidos {Count} arquivos de exportação antigos (> 7 dias)", deletedCount);
    }

    // Classe auxiliar para desserializar parâmetros
    private class ExportParameters
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? CategoryId { get; set; }
        public int? TransactionType { get; set; }
    }
}
