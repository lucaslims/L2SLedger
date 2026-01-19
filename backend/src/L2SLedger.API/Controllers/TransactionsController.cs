using L2SLedger.Application.DTOs.Transaction;
using L2SLedger.Application.UseCases.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Endpoints para gerenciamento de transações financeiras.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly CreateTransactionUseCase _createTransactionUseCase;
    private readonly UpdateTransactionUseCase _updateTransactionUseCase;
    private readonly GetTransactionsUseCase _getTransactionsUseCase;
    private readonly GetTransactionByIdUseCase _getTransactionByIdUseCase;
    private readonly DeleteTransactionUseCase _deleteTransactionUseCase;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        CreateTransactionUseCase createTransactionUseCase,
        UpdateTransactionUseCase updateTransactionUseCase,
        GetTransactionsUseCase getTransactionsUseCase,
        GetTransactionByIdUseCase getTransactionByIdUseCase,
        DeleteTransactionUseCase deleteTransactionUseCase,
        ILogger<TransactionsController> logger)
    {
        _createTransactionUseCase = createTransactionUseCase;
        _updateTransactionUseCase = updateTransactionUseCase;
        _getTransactionsUseCase = getTransactionsUseCase;
        _getTransactionByIdUseCase = getTransactionByIdUseCase;
        _deleteTransactionUseCase = deleteTransactionUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Lista transações com filtros e paginação.
    /// </summary>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10, máximo: 100)</param>
    /// <param name="categoryId">Filtrar por categoria (opcional)</param>
    /// <param name="type">Filtrar por tipo: 1=Receita, 2=Despesa (opcional)</param>
    /// <param name="startDate">Data inicial do período (opcional)</param>
    /// <param name="endDate">Data final do período (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de transações com totalizadores</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetTransactionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetTransactionsResponse>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] int? type = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _getTransactionsUseCase.ExecuteAsync(
                page, pageSize, categoryId, type, startDate, endDate, cancellationToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar transações");
            throw;
        }
    }

    /// <summary>
    /// Obtém uma transação por ID.
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados da transação</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TransactionDto>> GetTransactionById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var transaction = await _getTransactionByIdUseCase.ExecuteAsync(id, cancellationToken);

            if (transaction == null)
            {
                return NotFound(new { error = "Transação não encontrada" });
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter transação {TransactionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Cria uma nova transação.
    /// </summary>
    /// <param name="request">Dados da transação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>ID da transação criada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTransaction(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var transactionId = await _createTransactionUseCase.ExecuteAsync(request, cancellationToken);

            _logger.LogInformation("Transação criada com sucesso: {TransactionId}", transactionId);

            return CreatedAtAction(
                nameof(GetTransactionById),
                new { id = transactionId },
                new { id = transactionId });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Erro de validação ao criar transação: {Errors}", ex.Errors);
            return BadRequest(new
            {
                errors = ex.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de operação ao criar transação");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar transação");
            throw;
        }
    }

    /// <summary>
    /// Atualiza uma transação existente.
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="request">Dados atualizados</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Sem conteúdo</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateTransaction(
        Guid id,
        [FromBody] UpdateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _updateTransactionUseCase.ExecuteAsync(id, request, cancellationToken);

            _logger.LogInformation("Transação atualizada com sucesso: {TransactionId}", id);

            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Erro de validação ao atualizar transação {TransactionId}: {Errors}", id, ex.Errors);
            return BadRequest(new
            {
                errors = ex.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de operação ao atualizar transação {TransactionId}", id);
            
            if (ex.Message.Contains("não encontrada"))
            {
                return NotFound(new { error = ex.Message });
            }
            
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar transação {TransactionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Exclui uma transação (soft delete).
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Sem conteúdo</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteTransaction(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _deleteTransactionUseCase.ExecuteAsync(id, cancellationToken);

            _logger.LogInformation("Transação excluída com sucesso: {TransactionId}", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao excluir transação {TransactionId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir transação {TransactionId}", id);
            throw;
        }
    }
}
