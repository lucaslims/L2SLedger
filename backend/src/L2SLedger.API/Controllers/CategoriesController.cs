using L2SLedger.API.Contracts;
using L2SLedger.Application.DTOs.Categories;
using L2SLedger.Application.UseCases.Categories;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Endpoints para gerenciamento de categorias financeiras.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly CreateCategoryUseCase _createCategoryUseCase;
    private readonly UpdateCategoryUseCase _updateCategoryUseCase;
    private readonly GetCategoriesUseCase _getCategoriesUseCase;
    private readonly GetCategoryByIdUseCase _getCategoryByIdUseCase;
    private readonly DeactivateCategoryUseCase _deactivateCategoryUseCase;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        CreateCategoryUseCase createCategoryUseCase,
        UpdateCategoryUseCase updateCategoryUseCase,
        GetCategoriesUseCase getCategoriesUseCase,
        GetCategoryByIdUseCase getCategoryByIdUseCase,
        DeactivateCategoryUseCase deactivateCategoryUseCase,
        ILogger<CategoriesController> logger)
    {
        _createCategoryUseCase = createCategoryUseCase;
        _updateCategoryUseCase = updateCategoryUseCase;
        _getCategoriesUseCase = getCategoriesUseCase;
        _getCategoryByIdUseCase = getCategoryByIdUseCase;
        _deactivateCategoryUseCase = deactivateCategoryUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as categorias ou filtra por categoria pai e/ou tipo.
    /// </summary>
    /// <param name="parentCategoryId">ID da categoria pai (opcional, para listar subcategorias)</param>
    /// <param name="includeInactive">Incluir categorias inativas</param>
    /// <param name="type">Tipo da categoria: Income ou Expense (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de categorias</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetCategoriesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetCategoriesResponse>> GetCategories(
        [FromQuery] Guid? parentCategoryId = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? type = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _getCategoriesUseCase.ExecuteAsync(parentCategoryId, includeInactive, type, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar categorias");
            throw;
        }
    }

    /// <summary>
    /// Obtém uma categoria por ID.
    /// </summary>
    /// <param name="id">ID da categoria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados da categoria</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _getCategoryByIdUseCase.ExecuteAsync(id, cancellationToken);
            return Ok(category);
        }
        catch (BusinessRuleException ex) when (ex.Code == ErrorCodes.FIN_CATEGORY_NOT_FOUND)
        {
            return NotFound(ErrorResponse.Create(ex.Code, ex.Message, traceId: HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar categoria {CategoryId}", id);
            throw;
        }
    }

    /// <summary>
    /// Cria uma nova categoria.
    /// </summary>
    /// <param name="request">Dados da categoria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Categoria criada</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Financeiro")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _createCategoryUseCase.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = category.Id },
                category);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ErrorResponse.Create(
                ErrorCodes.VAL_VALIDATION_FAILED,
                "Falha na validação",
                details: string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")),
                traceId: HttpContext.TraceIdentifier));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Code, ex.Message, traceId: HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar categoria");
            throw;
        }
    }

    /// <summary>
    /// Atualiza uma categoria existente.
    /// </summary>
    /// <param name="id">ID da categoria</param>
    /// <param name="request">Novos dados da categoria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Categoria atualizada</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Financeiro")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        [FromRoute] Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _updateCategoryUseCase.ExecuteAsync(id, request, cancellationToken);
            return Ok(category);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(ErrorResponse.Create(
                ErrorCodes.VAL_VALIDATION_FAILED,
                "Falha na validação",
                details: string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")),
                traceId: HttpContext.TraceIdentifier));
        }
        catch (BusinessRuleException ex) when (ex.Code == ErrorCodes.FIN_CATEGORY_NOT_FOUND)
        {
            return NotFound(ErrorResponse.Create(ex.Code, ex.Message, traceId: HttpContext.TraceIdentifier));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Code, ex.Message, traceId: HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar categoria {CategoryId}", id);
            throw;
        }
    }

    /// <summary>
    /// Desativa uma categoria (soft delete).
    /// </summary>
    /// <param name="id">ID da categoria</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Status da operação</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Financeiro")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateCategory(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _deactivateCategoryUseCase.ExecuteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (BusinessRuleException ex) when (ex.Code == ErrorCodes.FIN_CATEGORY_NOT_FOUND)
        {
            return NotFound(ErrorResponse.Create(ex.Code, ex.Message, traceId: HttpContext.TraceIdentifier));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ErrorResponse.Create(ex.Code, ex.Message, traceId: HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar categoria {CategoryId}", id);
            throw;
        }
    }
}
