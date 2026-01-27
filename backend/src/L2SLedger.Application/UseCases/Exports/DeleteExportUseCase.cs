using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Exports;

/// <summary>
/// Caso de uso para deletar uma exportação (soft delete).
/// </summary>
public class DeleteExportUseCase
{
    private readonly IExportRepository _exportRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteExportUseCase> _logger;

    /// <summary>
    /// Construtor do caso de uso.
    /// </summary>
    /// <param name="exportRepository">Repositório de exportações.</param>
    /// <param name="fileStorageService">Serviço de armazenamento de arquivos.</param>
    /// <param name="currentUserService">Serviço para obter informações do usuário atual.</param>
    /// <param name="logger">Logger para registrar informações.</param>
    public DeleteExportUseCase(
        IExportRepository exportRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        ILogger<DeleteExportUseCase> logger)
    {
        _exportRepository = exportRepository;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Executa o caso de uso para deletar uma exportação.
    /// </summary>
    /// <param name="exportId">ID da exportação a ser deletada.</param>
    /// <exception cref="NotFoundException">Exceção lançada quando a exportação não é encontrada.</exception>
    /// <exception cref="AuthorizationException">Exceção lançada quando o usuário não está autorizado (não é Admin).</exception>
    public async Task ExecuteAsync(Guid exportId)
    {
        var userId = _currentUserService.GetUserId();
        var isAdmin = _currentUserService.IsInRole("Admin");

        // Apenas Admin pode deletar exportações
        if (!isAdmin)
        {
            _logger.LogWarning(
                "User {UserId} attempted to delete export {ExportId} without Admin role",
                userId,
                exportId
            );
            throw new AuthorizationException(ErrorCodes.EXPORT_DELETE_UNAUTHORIZED, "Only Admin users can delete exports");
        }

        var export = await _exportRepository.GetByIdAsync(exportId);

        if (export == null)
        {
            _logger.LogWarning("Export {ExportId} not found for deletion", exportId);
            throw new NotFoundException(ErrorCodes.EXPORT_NOT_FOUND, exportId.ToString());
        }

        // Deletar arquivo físico se existir
        if (!string.IsNullOrEmpty(export.FilePath))
        {
            try
            {
                await _fileStorageService.DeleteExportFileAsync(export.FilePath);
                _logger.LogInformation(
                    "Physical file deleted for export {ExportId}: {FilePath}",
                    exportId,
                    export.FilePath
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to delete physical file for export {ExportId}: {FilePath}",
                    exportId,
                    export.FilePath
                );
                // Continua com soft delete mesmo se falhar a deleção do arquivo
            }
        }

        // Soft delete da exportação
        await _exportRepository.DeleteAsync(export);

        _logger.LogInformation(
            "Export {ExportId} soft deleted by Admin user {UserId}",
            exportId,
            userId
        );
    }
}
