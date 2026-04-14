using AutoMapper;
using L2SLedger.Application.Common.Logging;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Users;

/// <summary>
/// Caso de uso para atualizar o status de um usuário.
/// Conforme user-status-plan.md e ADR-016: Apenas Admin pode executar.
/// Conforme ADR-014: Registra auditoria de alterações.
/// </summary>
public class UpdateUserStatusUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserStatusUseCase> _logger;

    public UpdateUserStatusUseCase(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper,
        ILogger<UpdateUserStatusUseCase> logger)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Executa a atualização do status de um usuário.
    /// </summary>
    /// <param name="userId">ID do usuário a atualizar.</param>
    /// <param name="request">Request com novo status e motivo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Usuário atualizado.</returns>
    /// <exception cref="BusinessRuleException">Quando violação de regra de negócio.</exception>
    public async Task<UserDetailDto> ExecuteAsync(
        Guid userId,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar request
        ValidateRequest(request);

        // 2. Verificar se admin está tentando modificar seu próprio status
        var currentUserId = _currentUserService.GetUserId();
        if (userId == currentUserId)
        {
            throw new BusinessRuleException(
                ErrorCodes.USER_CANNOT_MODIFY_OWN_STATUS,
                "Você não pode modificar seu próprio status. Solicite a outro administrador.");
        }

        // 3. Buscar usuário
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new BusinessRuleException(
                ErrorCodes.USER_NOT_FOUND,
                $"Usuário com ID {userId} não encontrado.");

        // 4. Capturar status anterior para auditoria
        var oldStatus = user.Status;

        // 5. Aplicar transição de status
        ApplyStatusTransition(user, request.Status);

        // 6. Persistir alterações
        await _userRepository.UpdateAsync(user, cancellationToken);

        // 7. Registrar auditoria (ADR-014)
        // Nota: Para auditoria adequada de status, criar objeto com contexto adicional
        var auditContext = new
        {
            user.Id,
            user.Email,
            OldStatus = oldStatus.ToString(),
            NewStatus = user.Status.ToString(),
            request.Reason,
            ModifiedBy = currentUserId,
            ModifiedAt = DateTime.UtcNow
        };
        await _auditService.LogUpdateAsync(auditContext, auditContext, cancellationToken);

        // 8. Log estruturado
        var sanitizedEmail = LogSanitizer.Sanitize(user.Email, maskEmail: true);
        var sanitizedReason = LogSanitizer.Sanitize(request.Reason);
        _logger.LogInformation(
            "Status do usuário {UserId} ({Email}) alterado de {OldStatus} para {NewStatus}. Motivo: {Reason}. Executado por Admin {AdminId}",
            userId,
            sanitizedEmail,
            oldStatus,
            user.Status,
            sanitizedReason,
            currentUserId);

        return _mapper.Map<UserDetailDto>(user);
    }

    private static void ValidateRequest(UpdateUserStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            throw new BusinessRuleException(
                ErrorCodes.USER_STATUS_REQUIRED,
                "Status é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new BusinessRuleException(
                ErrorCodes.USER_STATUS_REASON_REQUIRED,
                "É obrigatório informar o motivo da alteração de status.");
        }

        if (request.Reason.Length > 2000)
        {
            throw new BusinessRuleException(
                ErrorCodes.USER_STATUS_REASON_TOO_LONG,
                "O motivo não pode exceder 2000 caracteres.");
        }
    }

    private static void ApplyStatusTransition(Domain.Entities.User user, string newStatus)
    {
        try
        {
            switch (newStatus)
            {
                case "Active":
                    if (user.Status == UserStatus.Pending)
                    {
                        user.Approve();
                    }
                    else if (user.Status == UserStatus.Suspended)
                    {
                        user.Reactivate();
                    }
                    else
                    {
                        throw new InvalidStatusTransitionException(user.Status.ToString(), newStatus);
                    }
                    break;

                case "Suspended":
                    user.Suspend();
                    break;

                case "Rejected":
                    user.Reject();
                    break;

                default:
                    throw new BusinessRuleException(
                        ErrorCodes.USER_INVALID_STATUS,
                        $"Status '{newStatus}' inválido. Valores válidos: Active, Suspended, Rejected.");
            }
        }
        catch (InvalidStatusTransitionException ex)
        {
            throw new BusinessRuleException(ex.Code, ex.Message, ex);
        }
    }
}
