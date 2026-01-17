using AutoMapper;
using FluentValidation;
using L2SLedger.Application.DTOs.Adjustments;
using L2SLedger.Application.Interfaces;

namespace L2SLedger.Application.UseCases.Adjustments;

/// <summary>
/// Use case para buscar ajustes com filtros e paginação.
/// </summary>
public class GetAdjustmentsUseCase
{
    private readonly IAdjustmentRepository _adjustmentRepository;
    private readonly IValidator<GetAdjustmentsRequest> _validator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetAdjustmentsUseCase(
        IAdjustmentRepository adjustmentRepository,
        IValidator<GetAdjustmentsRequest> validator,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _adjustmentRepository = adjustmentRepository;
        _validator = validator;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<GetAdjustmentsResponse> ExecuteAsync(GetAdjustmentsRequest request, CancellationToken cancellationToken = default)
    {
        // Validação
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Obter usuário autenticado
        var userId = _currentUserService.GetUserId();

        // Buscar ajustes com filtros
        var (adjustments, totalCount) = await _adjustmentRepository.GetByFiltersAsync(
            userId: userId,
            page: request.Page,
            pageSize: request.PageSize,
            originalTransactionId: request.OriginalTransactionId,
            type: request.Type.HasValue ? (Domain.Entities.AdjustmentType)request.Type.Value : null,
            startDate: request.StartDate,
            endDate: request.EndDate,
            createdByUserId: request.CreatedByUserId,
            includeDeleted: request.IncludeDeleted,
            cancellationToken: cancellationToken
        );

        // Mapear para DTOs
        var adjustmentDtos = _mapper.Map<List<AdjustmentDto>>(adjustments);

        return new GetAdjustmentsResponse
        {
            Adjustments = adjustmentDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
