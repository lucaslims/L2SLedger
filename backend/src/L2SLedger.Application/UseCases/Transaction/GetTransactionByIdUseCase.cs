using AutoMapper;
using L2SLedger.Application.DTOs.Transaction;
using L2SLedger.Application.Interfaces;

namespace L2SLedger.Application.UseCases.Transaction;

/// <summary>
/// Use case para obter transação por ID.
/// </summary>
public class GetTransactionByIdUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetTransactionByIdUseCase(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<TransactionDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Obter usuário autenticado
        var userId = _currentUserService.GetUserId();

        // Buscar transação
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null || transaction.UserId != userId)
        {
            return null;
        }

        // Mapear para DTO
        return _mapper.Map<TransactionDto>(transaction);
    }
}
