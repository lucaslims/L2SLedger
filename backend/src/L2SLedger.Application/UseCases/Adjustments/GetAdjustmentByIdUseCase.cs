using AutoMapper;
using L2SLedger.Application.DTOs.Adjustments;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Application.UseCases.Adjustments;

/// <summary>
/// Use case para buscar um ajuste específico por ID.
/// </summary>
public class GetAdjustmentByIdUseCase
{
    private readonly IAdjustmentRepository _adjustmentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetAdjustmentByIdUseCase(
        IAdjustmentRepository adjustmentRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _adjustmentRepository = adjustmentRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<AdjustmentDto> ExecuteAsync(Guid adjustmentId, CancellationToken cancellationToken = default)
    {
        // Obter usuário autenticado
        var userId = _currentUserService.GetUserId();

        // Buscar ajuste
        var adjustment = await _adjustmentRepository.GetByIdAsync(adjustmentId, cancellationToken);

        if (adjustment == null)
        {
            throw new BusinessRuleException(
                "FIN_ADJUSTMENT_NOT_FOUND",
                "Ajuste não encontrado");
        }

        // Verificar se o ajuste pertence ao usuário (via transação original)
        if (adjustment.OriginalTransaction.UserId != userId)
        {
            throw new BusinessRuleException(
                "FIN_ADJUSTMENT_UNAUTHORIZED",
                "Você não tem permissão para visualizar este ajuste");
        }

        // Mapear e retornar
        return _mapper.Map<AdjustmentDto>(adjustment);
    }
}
