using L2SLedger.Application.DTOs.Exports;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Exports;

/// <summary>
/// Caso de uso para obter o status de uma exportação.
/// </summary>
public class GetExportStatusUseCase
{
    private readonly IExportRepository _exportRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetExportStatusUseCase> _logger;

    /// <summary>
    /// Construtor do caso de uso.
    /// </summary>
    /// <param name="exportRepository">Repositório de exportações.</param>
    /// <param name="currentUserService">Serviço para obter informações do usuário atual.</param>
    /// <param name="logger">Logger para registrar informações e erros.</param>
    public GetExportStatusUseCase(
        IExportRepository exportRepository,
        ICurrentUserService currentUserService,
        ILogger<GetExportStatusUseCase> logger)
    {
        _exportRepository = exportRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Executa o caso de uso para obter o status de uma exportação.    
    /// </summary>
    /// <param name="exportId">ID da exportação.</param>
    /// <returns>Resposta com o status da exportação.</returns>
    /// <exception cref="NotFoundException">Exceção lançada quando a exportação não é encontrada.</exception>
    /// <exception cref="AuthorizationException">Exceção lançada quando o usuário não está autorizado a acessar a exportação.</exception>
    public async Task<ExportStatusResponse> ExecuteAsync(Guid exportId)
    {
        var userId = _currentUserService.GetUserId();
        var export = await _exportRepository.GetByIdAsync(exportId);

        if (export == null)
            throw new NotFoundException(ErrorCodes.EXPORT_NOT_FOUND, $"Export with ID {exportId} not found.");

        // Validar ownership (usuário só vê suas próprias exportações)
        if (export.RequestedByUserId != userId && !_currentUserService.IsInRole("Admin"))
            throw new AuthorizationException(ErrorCodes.EXPORT_UNAUTHORIZED, "You are not authorized to view this export.");

        // Calcular progresso baseado no status
        int? progressPercentage = export.Status switch
        {
            ExportStatus.Pending => 0,
            ExportStatus.Processing => 50,
            ExportStatus.Completed => 100,
            ExportStatus.Failed => 100,
            _ => null
        };

        return new ExportStatusResponse
        {
            Id = export.Id,
            Status = export.Status.ToString(),
            ErrorMessage = export.ErrorMessage,
            ProgressPercentage = progressPercentage,
            RequestedAt = export.RequestedAt,
            CompletedAt = export.CompletedAt,
            IsDownloadable = export.IsDownloadable()
        };
    }
}
