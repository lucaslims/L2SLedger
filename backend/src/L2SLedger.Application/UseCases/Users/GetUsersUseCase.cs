using AutoMapper;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.Interfaces;

namespace L2SLedger.Application.UseCases.Users;

/// <summary>
/// Caso de uso para listar usuários com paginação e filtros.
/// Conforme ADR-016: Apenas Admin pode executar.
/// </summary>
public class GetUsersUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersUseCase(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Executa a listagem de usuários com paginação e filtros.
    /// </summary>
    /// <param name="request">Parâmetros de paginação e filtros.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Response paginado com usuários.</returns>
    public async Task<GetUsersResponse> ExecuteAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validar e normalizar limites de paginação
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var (users, totalCount) = await _userRepository.GetAllAsync(
            page,
            pageSize,
            request.Email,
            request.Role,
            request.IncludeInactive,
            cancellationToken);

        var userDtos = _mapper.Map<IReadOnlyList<UserSummaryDto>>(users);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new GetUsersResponse
        {
            Users = userDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
