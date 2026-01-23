using AutoMapper;
using FluentValidation;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.UseCases.Audit;

/// <summary>
/// Use case para listar eventos de auditoria com filtros.
/// Apenas Admin pode acessar (validado no Controller).
/// </summary>
public class GetAuditEventsUseCase
{
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly IValidator<GetAuditEventsRequest> _validator;
    private readonly IMapper _mapper;

    public GetAuditEventsUseCase(
        IAuditEventRepository auditEventRepository,
        IValidator<GetAuditEventsRequest> validator,
        IMapper mapper)
    {
        _auditEventRepository = auditEventRepository;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<GetAuditEventsResponse> ExecuteAsync(
        GetAuditEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validação
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Converter tipo se informado
        AuditEventType? eventType = request.EventType.HasValue
            ? (AuditEventType)request.EventType.Value
            : null;

        // Buscar eventos
        var (events, totalCount) = await _auditEventRepository.GetByFiltersAsync(
            page: request.Page,
            pageSize: request.PageSize,
            eventType: eventType,
            entityType: request.EntityType,
            entityId: request.EntityId,
            userId: request.UserId,
            startDate: request.StartDate,
            endDate: request.EndDate,
            result: request.Result,
            cancellationToken: cancellationToken
        );

        // Mapear para DTOs
        var eventDtos = _mapper.Map<List<AuditEventDto>>(events);

        return new GetAuditEventsResponse
        {
            Events = eventDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
