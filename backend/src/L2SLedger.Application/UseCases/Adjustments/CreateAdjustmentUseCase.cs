using AutoMapper;
using FluentValidation;
using L2SLedger.Application.DTOs.Adjustments;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Application.UseCases.Adjustments;

/// <summary>
/// Use case para criar um ajuste pós-fechamento.
/// Conforme ADR-015 (Imutabilidade e Ajustes Pós-Fechamento).
/// </summary>
public class CreateAdjustmentUseCase
{
    private readonly IAdjustmentRepository _adjustmentRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IValidator<CreateAdjustmentRequest> _validator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreateAdjustmentUseCase(
        IAdjustmentRepository adjustmentRepository,
        ITransactionRepository transactionRepository,
        IValidator<CreateAdjustmentRequest> validator,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _adjustmentRepository = adjustmentRepository;
        _transactionRepository = transactionRepository;
        _validator = validator;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<AdjustmentDto> ExecuteAsync(CreateAdjustmentRequest request, CancellationToken cancellationToken = default)
    {
        // Validação
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Obter usuário autenticado
        var userId = _currentUserService.GetUserId();

        // Verificar se a transação original existe
        var originalTransaction = await _transactionRepository.GetByIdAsync(request.OriginalTransactionId, cancellationToken);
        if (originalTransaction == null)
        {
            throw new BusinessRuleException(
                ErrorCodes.FIN_ADJUSTMENT_INVALID_ORIGINAL,
                "Transação original não encontrada");
        }

        // Verificar se a transação original pertence ao usuário
        if (originalTransaction.UserId != userId)
        {
            throw new BusinessRuleException(
                ErrorCodes.FIN_ADJUSTMENT_UNAUTHORIZED,
                "Você não tem permissão para ajustar esta transação");
        }

        // Criar entidade de ajuste
        var adjustmentDate = request.AdjustmentDate ?? DateTime.UtcNow.Date;
        var adjustment = new Adjustment(
            originalTransactionId: request.OriginalTransactionId,
            amount: request.Amount,
            type: (AdjustmentType)request.Type,
            reason: request.Reason,
            adjustmentDate: adjustmentDate,
            createdByUserId: userId
        );

        // Validar regras de negócio específicas
        adjustment.ValidateAgainstOriginal(originalTransaction);

        // Persistir
        await _adjustmentRepository.AddAsync(adjustment, cancellationToken);

        // Retornar DTO
        var adjustmentDto = _mapper.Map<AdjustmentDto>(adjustment);
        adjustmentDto.OriginalTransactionDescription = originalTransaction.Description;
        adjustmentDto.CreatedByUserName = _currentUserService.GetUserName() ?? originalTransaction.User.DisplayName;

        return adjustmentDto;
    }
}
