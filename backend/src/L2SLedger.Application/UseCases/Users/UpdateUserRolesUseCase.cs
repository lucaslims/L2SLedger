using AutoMapper;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;
using L2SLedger.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Users;

/// <summary>
/// Caso de uso para atualizar roles de um usuário.
/// Conforme ADR-016: Apenas Admin pode executar.
/// Conforme ADR-014: Registra auditoria de alterações.
/// </summary>
public class UpdateUserRolesUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateUserRolesUseCase> _logger;

    public UpdateUserRolesUseCase(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UpdateUserRolesUseCase> logger)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Executa a atualização de roles de um usuário.
    /// </summary>
    /// <param name="userId">ID do usuário a atualizar.</param>
    /// <param name="request">Request com novas roles.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Usuário atualizado.</returns>
    /// <exception cref="ValidationException">Quando roles inválidos.</exception>
    /// <exception cref="BusinessRuleException">Quando violação de regra de negócio.</exception>
    public async Task<UserDetailDto> ExecuteAsync(
        Guid userId,
        UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar roles fornecidos
        ValidateRoles(request.Roles);

        // 2. Buscar usuário
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new BusinessRuleException(
                "USER_NOT_FOUND",
                $"Usuário com ID {userId} não encontrado.");

        // 3. Obter usuário atual (Admin executando a ação)
        var currentUserId = _currentUserService.GetUserId();

        // 4. Validar regras de negócio
        await ValidateBusinessRulesAsync(user, request.Roles, currentUserId, cancellationToken);

        // 5. Capturar roles anteriores para auditoria
        var oldRoles = user.Roles.ToList();

        // 6. Atualizar roles
        UpdateRoles(user, request.Roles);

        // 7. Persistir alterações
        await _userRepository.UpdateAsync(user, cancellationToken);

        // 8. Log de auditoria (ADR-014)
        _logger.LogInformation(
            "Roles do usuário {UserId} ({Email}) atualizados de [{OldRoles}] para [{NewRoles}] por Admin {AdminId}",
            userId,
            user.Email,
            string.Join(", ", oldRoles),
            string.Join(", ", request.Roles),
            currentUserId);

        return _mapper.Map<UserDetailDto>(user);
    }

    private static void ValidateRoles(IReadOnlyList<string> roles)
    {
        if (roles is null || roles.Count == 0)
        {
            throw new BusinessRuleException(
                "ROLES_REQUIRED",
                "Pelo menos uma role deve ser especificada.");
        }

        foreach (var role in roles)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new BusinessRuleException(
                    "ROLE_EMPTY",
                    "Role não pode ser vazio.");
            }

            if (!Role.IsValid(role))
            {
                throw new BusinessRuleException(
                    "INVALID_ROLE",
                    $"Role inválido: {role}. Valores permitidos: {string.Join(", ", Role.GetAllRoles())}");
            }
        }
    }

    private async Task ValidateBusinessRulesAsync(
        Domain.Entities.User user,
        IReadOnlyList<string> newRoles,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        // Regra 1: Admin não pode remover seu próprio papel de Admin
        var isRemovingAdminFromSelf = user.Id == currentUserId
            && user.IsAdmin()
            && !newRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase);

        if (isRemovingAdminFromSelf)
        {
            throw new BusinessRuleException(
                "CANNOT_REMOVE_OWN_ADMIN",
                "Você não pode remover seu próprio papel de Admin.");
        }

        // Regra 2: Pelo menos um Admin deve existir no sistema
        if (user.IsAdmin() && !newRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
        {
            var existsOtherAdmin = await _userRepository.ExistsOtherAdminAsync(user.Id, cancellationToken);

            if (!existsOtherAdmin)
            {
                throw new BusinessRuleException(
                    "LAST_ADMIN",
                    "Não é possível remover o papel de Admin do último administrador do sistema.");
            }
        }
    }

    private static void UpdateRoles(Domain.Entities.User user, IReadOnlyList<string> newRoles)
    {
        // Remover roles atuais que não estão na nova lista
        var rolesToRemove = user.Roles
            .Where(r => !newRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
            .ToList();

        foreach (var role in rolesToRemove)
        {
            user.RemoveRole(role);
        }

        // Adicionar novas roles que não existem
        foreach (var role in newRoles)
        {
            if (!user.HasRole(role))
            {
                user.AddRole(role);
            }
        }
    }
}
