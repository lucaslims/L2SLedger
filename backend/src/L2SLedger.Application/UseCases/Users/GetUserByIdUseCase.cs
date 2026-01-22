using AutoMapper;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Application.UseCases.Users;

/// <summary>
/// Caso de uso para obter detalhes de um usuário por ID.
/// Conforme ADR-016: Apenas Admin pode executar.
/// </summary>
public class GetUserByIdUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserByIdUseCase(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Executa a consulta de um usuário por ID.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Detalhes do usuário.</returns>
    /// <exception cref="BusinessRuleException">Quando usuário não encontrado.</exception>
    public async Task<UserDetailDto> ExecuteAsync(
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

        return _mapper.Map<UserDetailDto>(user);
    }
}
