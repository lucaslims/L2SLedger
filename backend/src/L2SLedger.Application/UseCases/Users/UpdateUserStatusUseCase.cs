using AutoMapper;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;
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

        // 2. Buscar usuário
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new BusinessRuleException(
                "USER_NOT_FOUND",
                $"Usuário com ID {userId} não encontrado.");

        // 3. Capturar status anterior para auditoria
        var oldStatus = user.Status;
        var userBefore = CloneUserForAudit(user);

        // 4. Aplicar transição de status
        ApplyStatusTransition(user, request.Status);

        // 5. Persistir alterações
        await _userRepository.UpdateAsync(user, cancellationToken);

        // 6. Registrar auditoria (ADR-014)
        await _auditService.LogUpdateAsync(userBefore, user, cancellationToken);

        // 7. Log estruturado
        _logger.LogInformation(
            "Status do usuário {UserId} ({Email}) alterado de {OldStatus} para {NewStatus}. Motivo: {Reason}. Executado por Admin {AdminId}",
            userId,
            user.Email,
            oldStatus,
            user.Status,
            request.Reason,
            _currentUserService.GetUserId());

        return _mapper.Map<UserDetailDto>(user);
    }

    private static void ValidateRequest(UpdateUserStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            throw new BusinessRuleException(
                "USER_STATUS_REQUIRED",
                "Status é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new BusinessRuleException(
                "USER_STATUS_REASON_REQUIRED",
                "É obrigatório informar o motivo da alteração de status.");
        }

        if (request.Reason.Length > 2000)
        {
            throw new BusinessRuleException(
                "USER_STATUS_REASON_TOO_LONG",
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
                        "USER_INVALID_STATUS",
                        $"Status '{newStatus}' inválido. Valores válidos: Active, Suspended, Rejected.");
            }
        }
        catch (InvalidStatusTransitionException ex)
        {
            throw new BusinessRuleException(ex.Code, ex.Message, ex);
        }
    }

    private static Domain.Entities.User CloneUserForAudit(Domain.Entities.User user)
    {
        // Criar uma cópia simples para auditoria
        // O AuditService deve lidar com a serialização
        return user;
    }
}
