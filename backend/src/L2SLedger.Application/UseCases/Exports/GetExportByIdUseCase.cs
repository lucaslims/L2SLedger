using AutoMapper;
using L2SLedger.Application.DTOs.Exports;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Exports;

/// <summary>
/// Caso de uso para obter uma exportação por ID.
/// </summary>
public class GetExportByIdUseCase
{
    private readonly IExportRepository _exportRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetExportByIdUseCase> _logger;

    /// <summary>
    /// Construtor do caso de uso.
    /// </summary>
    /// <param name="exportRepository">Repositório de exportações.</param>
    /// <param name="currentUserService">Serviço para obter informações do usuário atual.</param>
    /// <param name="mapper">Mapeador de objetos.</param>
    /// <param name="logger">Logger para registrar informações.</param>
    public GetExportByIdUseCase(
        IExportRepository exportRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetExportByIdUseCase> logger)
    {
        _exportRepository = exportRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Executa o caso de uso para obter uma exportação por ID.
    /// </summary>
    /// <param name="exportId">ID da exportação a ser obtida.</param>
    /// <returns>DTO da exportação.</returns>
    /// <exception cref="NotFoundException">Exceção lançada quando a exportação não é encontrada.</exception>
    /// <exception cref="AuthorizationException">Exceção lançada quando o usuário não está autorizado a acessar a exportação.</exception>
    public async Task<ExportDto> ExecuteAsync(Guid exportId)
    {
        var userId = _currentUserService.GetUserId();
        var export = await _exportRepository.GetByIdAsync(exportId);

        if (export == null)
            throw new NotFoundException("EXPORT_NOT_FOUND", $"Export with ID {exportId} not found.");

        // Validar ownership
        if (export.RequestedByUserId != userId && !_currentUserService.IsInRole("Admin"))
            throw new AuthorizationException("EXPORT_UNAUTHORIZED", "You are not authorized to view this export.");

        return new ExportDto
        {
            Id = export.Id,
            ExportType = export.ExportType,
            Format = export.Format.ToString(),
            Status = export.Status.ToString(),
            FilePath = export.FilePath,
            FileSizeBytes = export.FileSizeBytes,
            ParametersJson = export.ParametersJson,
            RequestedByUserId = export.RequestedByUserId,
            RequestedByUserName = export.RequestedByUser?.DisplayName,
            RequestedAt = export.RequestedAt,
            ProcessingStartedAt = export.ProcessingStartedAt,
            CompletedAt = export.CompletedAt,
            ErrorMessage = export.ErrorMessage,
            RecordCount = export.RecordCount,
            CreatedAt = export.CreatedAt
        };
    }
}
