using L2SLedger.Application.DTOs.Exports;
using L2SLedger.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Exports;

/// <summary>
/// Caso de uso para listar exportações do usuário com filtros.
/// </summary>
public class GetExportsUseCase
{
    private readonly IExportRepository _exportRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetExportsUseCase> _logger;

    /// <summary>
    /// Construtor do caso de uso.
    /// </summary>
    /// <param name="exportRepository">Repositório de exportações.</param>
    /// <param name="currentUserService">Serviço para obter o usuário atual.</param>
    /// <param name="logger">Logger para registrar informações.</param>
    public GetExportsUseCase(
        IExportRepository exportRepository,
        ICurrentUserService currentUserService,
        ILogger<GetExportsUseCase> logger)
    {
        _exportRepository = exportRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Executa o caso de uso para listar exportações.
    /// </summary>
    /// <param name="request">Parâmetros de filtro e paginação.</param>
    /// <returns>Lista paginada de exportações.</returns>
    public async Task<GetExportsResponse> ExecuteAsync(GetExportsRequest request)
    {
        var userId = _currentUserService.GetUserId();
        var isAdmin = _currentUserService.IsInRole("Admin");

        // Admin pode ver todas as exportações, usuário comum apenas as suas
        var targetUserId = isAdmin ? Guid.Empty : userId;

        // Se não é Admin, sempre busca apenas as exportações do próprio usuário
        if (!isAdmin)
        {
            targetUserId = userId;
        }
        else
        {
            // Admin pode ver todas (Guid.Empty será tratado no repositório)
            targetUserId = userId; // Manteremos userId, repositório deve adaptar
        }

        // Buscar exportações com filtros
        var exports = await _exportRepository.GetByFiltersAsync(
            userId: isAdmin ? Guid.Empty : userId, // Guid.Empty = todas
            status: request.Status,
            format: request.Format,
            page: request.Page,
            pageSize: request.PageSize
        );

        var totalCount = await _exportRepository.CountByFiltersAsync(
            userId: isAdmin ? Guid.Empty : userId,
            status: request.Status,
            format: request.Format
        );

        _logger.LogInformation(
            "User {UserId} retrieved {Count} exports (Page {Page}/{TotalPages})",
            userId,
            exports.Count,
            request.Page,
            (int)Math.Ceiling((double)totalCount / request.PageSize)
        );

        return new GetExportsResponse
        {
            Exports = exports.Select(e => new ExportDto
            {
                Id = e.Id,
                ExportType = e.ExportType,
                Format = e.Format.ToString(),
                Status = e.Status.ToString(),
                FilePath = e.FilePath,
                FileSizeBytes = e.FileSizeBytes,
                ParametersJson = e.ParametersJson,
                RequestedByUserId = e.RequestedByUserId,
                RequestedByUserName = e.RequestedByUser?.DisplayName,
                RequestedAt = e.RequestedAt,
                ProcessingStartedAt = e.ProcessingStartedAt,
                CompletedAt = e.CompletedAt,
                ErrorMessage = e.ErrorMessage,
                RecordCount = e.RecordCount,
                CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
