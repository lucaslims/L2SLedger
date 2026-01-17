using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Application.UseCases.Adjustments;

/// <summary>
/// Use case para deletar (soft delete) um ajuste.
/// Apenas usuários Admin podem executar esta operação.
/// </summary>
public class DeleteAdjustmentUseCase
{
    private readonly IAdjustmentRepository _adjustmentRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteAdjustmentUseCase(
        IAdjustmentRepository adjustmentRepository,
        ICurrentUserService currentUserService)
    {
        _adjustmentRepository = adjustmentRepository;
        _currentUserService = currentUserService;
    }

    public async Task ExecuteAsync(Guid adjustmentId, CancellationToken cancellationToken = default)
    {
        // Verificar se o usuário é Admin
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new BusinessRuleException(
                "AUTH_INSUFFICIENT_PERMISSIONS",
                "Apenas administradores podem deletar ajustes");
        }

        // Buscar ajuste
        var adjustment = await _adjustmentRepository.GetByIdAsync(adjustmentId, cancellationToken);

        if (adjustment == null)
        {
            throw new BusinessRuleException(
                "FIN_ADJUSTMENT_NOT_FOUND",
                "Ajuste não encontrado");
        }

        if (adjustment.IsDeleted)
        {
            throw new BusinessRuleException(
                "FIN_ADJUSTMENT_ALREADY_DELETED",
                "Ajuste já foi excluído");
        }

        // Marcar como excluído
        adjustment.MarkAsDeleted();

        // Persistir
        await _adjustmentRepository.DeleteAsync(adjustment, cancellationToken);
    }
}
