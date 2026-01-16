using AutoMapper;
using L2SLedger.Application.DTOs.Transaction;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.UseCases.Transaction;

/// <summary>
/// Use case para listagem de transações com filtros e paginação.
/// </summary>
public class GetTransactionsUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetTransactionsUseCase(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<GetTransactionsResponse> ExecuteAsync(
        int page = 1,
        int pageSize = 10,
        Guid? categoryId = null,
        int? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Validar parâmetros
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        // Obter usuário autenticado
        var userId = _currentUserService.GetUserId();

        // Buscar transações
        var (transactions, totalCount) = await _transactionRepository.GetByFiltersAsync(
            userId: userId,
            page: page,
            pageSize: pageSize,
            categoryId: categoryId,
            type: type.HasValue ? (TransactionType?)type.Value : null,
            startDate: startDate,
            endDate: endDate,
            cancellationToken: cancellationToken);

        // Calcular totais
        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var balance = totalIncome - totalExpense;

        // Mapear para DTOs
        var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);

        return new GetTransactionsResponse
        {
            Transactions = transactionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = balance
        };
    }
}
