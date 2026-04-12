using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Common.Logging;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Exports;

/// <summary>
/// Caso de uso para baixar uma exportação.
/// </summary>
public class DownloadExportUseCase
{
    private readonly IExportRepository _exportRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DownloadExportUseCase> _logger;

    /// <summary>
    /// Construtor do caso de uso.
    /// </summary>
    /// <param name="exportRepository">Repositório de exportações.</param>
    /// <param name="fileStorageService">Serviço de armazenamento de arquivos.</param>
    /// <param name="currentUserService">Serviço para obter informações do usuário atual.</param>
    /// <param name="logger">Logger para registrar informações.</param>
    public DownloadExportUseCase(
        IExportRepository exportRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        ILogger<DownloadExportUseCase> logger)
    {
        _exportRepository = exportRepository;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Executa o caso de uso para baixar uma exportação.
    /// </summary>
    /// <param name="exportId">ID da exportação a ser baixada.</param>
    /// <returns>Tupla contendo os bytes do arquivo, nome do arquivo e tipo de conteúdo.</returns>
    /// <exception cref="NotFoundException">Exceção lançada quando a exportação não é encontrada.</exception>
    /// <exception cref="AuthorizationException">Exceção lançada quando o usuário não tem autorização para baixar a exportação.</exception>
    /// <exception cref="BusinessRuleException">Exceção lançada quando a exportação não está pronta para download.</exception>
    public async Task<(byte[] FileBytes, string FileName, string ContentType)> ExecuteAsync(Guid exportId)
    {
        var userId = _currentUserService.GetUserId();
        var export = await _exportRepository.GetByIdAsync(exportId);

        if (export == null)
            throw new NotFoundException(ErrorCodes.EXPORT_NOT_FOUND, $"Export with ID {exportId} not found.");

        // Validar ownership
        if (export.RequestedByUserId != userId && !_currentUserService.IsInRole("Admin"))
            throw new AuthorizationException(ErrorCodes.EXPORT_UNAUTHORIZED, "You are not authorized to download this export.");

        // Validar se pode ser baixado
        if (!export.IsDownloadable())
            throw new BusinessRuleException(
                ErrorCodes.EXPORT_NOT_COMPLETED,
                "Export is not ready for download. Current status: " + export.Status
            );

        // Ler arquivo
        var fileBytes = await _fileStorageService.ReadExportFileAsync(export.FilePath!);

        // Determinar content type e nome do arquivo
        var extension = export.Format == ExportFormat.Csv ? "csv" : "pdf";
        var contentType = export.Format == ExportFormat.Csv
            ? "text/csv"
            : "application/pdf";

        var userIdFull = userId.ToString("N");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"transactions_{userIdFull}_{timestamp}_{uniqueSuffix}.{extension}";

        _logger.LogInformation(
            "Export {ExportId} downloaded by user {UserId}. FileName={FileName}",
            exportId,
            userId,
            LogSanitizer.Sanitize(fileName)
        );

        return (fileBytes, fileName, contentType);
    }
}
