using System.Text.Json;
using L2SLedger.Application.Common.Logging;
using L2SLedger.Application.DTOs.Exports;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Exports;

/// <summary>
/// Caso de uso para solicitar uma nova exportação.
/// </summary>
public class RequestExportUseCase
{
    private readonly IExportRepository _exportRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RequestExportUseCase> _logger;

    /// <summary>
    /// Construtor do caso de uso.
    /// </summary>
    /// <param name="exportRepository">Repositório de exportações.</param>
    /// <param name="currentUserService">Serviço para obter o usuário atual.</param>
    /// <param name="logger">Logger para registrar informações.</param>
    public RequestExportUseCase(
        IExportRepository exportRepository,
        ICurrentUserService currentUserService,
        ILogger<RequestExportUseCase> logger)
    {
        _exportRepository = exportRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Executa o caso de uso para solicitar uma exportação.
    /// </summary>
    /// <param name="request">Dados da solicitação de exportação.</param>
    /// <returns>Detalhes da exportação criada.</returns>
    public async Task<ExportDto> ExecuteAsync(RequestExportRequest request)
    {
        var userId = _currentUserService.GetUserId();

        // Serializar parâmetros
        var parametersJson = JsonSerializer.Serialize(new
        {
            request.StartDate,
            request.EndDate,
            request.CategoryId,
            request.TransactionType
        });

        // Criar exportação
        var export = new Export(
            exportType: "Transactions",
            format: (ExportFormat)request.Format,
            parametersJson: parametersJson,
            requestedByUserId: userId
        );

        var created = await _exportRepository.AddAsync(export);

        _logger.LogInformation(
            "Export {ExportId} requested by user {UserId}. Format: {Format}",
            created.Id,
            userId,
            LogSanitizer.Sanitize(((ExportFormat)request.Format).ToString())
        );

        return new ExportDto
        {
            Id = created.Id,
            ExportType = created.ExportType,
            Format = created.Format.ToString(),
            Status = created.Status.ToString(),
            ParametersJson = created.ParametersJson,
            RequestedByUserId = created.RequestedByUserId,
            RequestedAt = created.RequestedAt,
            CreatedAt = created.CreatedAt
        };
    }
}
