using L2SLedger.Application.DTOs.Exports;
using L2SLedger.Application.UseCases.Exports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Controller para gerenciar exportações de relatórios.
/// Conforme ADR-017 (Exportação) e ADR-016 (RBAC).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ExportsController : ControllerBase
{
    private readonly RequestExportUseCase _requestExportUseCase;
    private readonly GetExportStatusUseCase _getExportStatusUseCase;
    private readonly GetExportByIdUseCase _getExportByIdUseCase;
    private readonly DownloadExportUseCase _downloadExportUseCase;
    private readonly GetExportsUseCase _getExportsUseCase;
    private readonly DeleteExportUseCase _deleteExportUseCase;
    private readonly ILogger<ExportsController> _logger;

    public ExportsController(
        RequestExportUseCase requestExportUseCase,
        GetExportStatusUseCase getExportStatusUseCase,
        GetExportByIdUseCase getExportByIdUseCase,
        DownloadExportUseCase downloadExportUseCase,
        GetExportsUseCase getExportsUseCase,
        DeleteExportUseCase deleteExportUseCase,
        ILogger<ExportsController> logger)
    {
        _requestExportUseCase = requestExportUseCase;
        _getExportStatusUseCase = getExportStatusUseCase;
        _getExportByIdUseCase = getExportByIdUseCase;
        _downloadExportUseCase = downloadExportUseCase;
        _getExportsUseCase = getExportsUseCase;
        _deleteExportUseCase = deleteExportUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Solicitar nova exportação de transações.
    /// </summary>
    /// <param name="request">Parâmetros da exportação (formato, período, filtros)</param>
    /// <returns>ID da exportação criada</returns>
    [HttpPost("transactions")]
    [ProducesResponseType(typeof(ExportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestExport([FromBody] RequestExportRequest request)
    {
        var export = await _requestExportUseCase.ExecuteAsync(request);
        _logger.LogInformation("Exportação solicitada: {ExportId}, formato: {Format}", export.Id, request.Format);

        return CreatedAtAction(nameof(GetExportById), new { id = export.Id }, export);
    }

    /// <summary>
    /// Consultar status de uma exportação específica.
    /// </summary>
    /// <param name="id">ID da exportação</param>
    /// <returns>Status da exportação</returns>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(ExportStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExportStatus(Guid id)
    {
        var status = await _getExportStatusUseCase.ExecuteAsync(id);
        return Ok(status);
    }

    /// <summary>
    /// Obter detalhes completos de uma exportação.
    /// </summary>
    /// <param name="id">ID da exportação</param>
    /// <returns>Detalhes da exportação</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExportById(Guid id)
    {
        var export = await _getExportByIdUseCase.ExecuteAsync(id);
        return Ok(export);
    }

    /// <summary>
    /// Download do arquivo da exportação (somente quando Status = Completed).
    /// </summary>
    /// <param name="id">ID da exportação</param>
    /// <returns>Arquivo da exportação</returns>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DownloadExport(Guid id)
    {
        var (fileBytes, fileName, contentType) = await _downloadExportUseCase.ExecuteAsync(id);

        _logger.LogInformation("Download da exportação {ExportId} - {FileName}", id, fileName);

        return File(fileBytes, contentType, fileName);
    }

    /// <summary>
    /// Listar exportações do usuário (com paginação e filtros).
    /// </summary>
    /// <param name="request">Filtros e paginação</param>
    /// <returns>Lista paginada de exportações</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetExportsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExports([FromQuery] GetExportsRequest request)
    {
        var response = await _getExportsUseCase.ExecuteAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Excluir uma exportação (soft delete).
    /// Apenas Admin pode executar.
    /// </summary>
    /// <param name="id">ID da exportação</param>
    /// <returns>Confirmação de exclusão</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteExport(Guid id)
    {
        await _deleteExportUseCase.ExecuteAsync(id);
        _logger.LogInformation("Exportação {ExportId} marcada para exclusão", id);

        return NoContent();
    }
}
