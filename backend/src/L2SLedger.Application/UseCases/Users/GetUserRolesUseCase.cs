using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;
using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.UseCases.Users;

/// <summary>
/// Caso de uso para obter roles de um usuário.
/// Conforme ADR-016: Apenas Admin pode executar.
/// </summary>
public class GetUserRolesUseCase
{
    private readonly IUserRepository _userRepository;

    public GetUserRolesUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Executa a consulta de roles de um usuário.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Roles do usuário e roles disponíveis.</returns>
    /// <exception cref="BusinessRuleException">Quando usuário não encontrado.</exception>
    public async Task<UserRolesResponse> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new BusinessRuleException(
                "USER_NOT_FOUND",
                $"Usuário com ID {userId} não encontrado.");
        }

        return new UserRolesResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = user.Roles.ToList(),
            AvailableRoles = Role.GetAllRoles().ToList()
        };
    }
}
