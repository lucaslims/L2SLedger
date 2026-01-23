using AutoMapper;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Application.UseCases.Audit;

/// <summary>
/// Use case para obter detalhes de um evento de auditoria.
/// Apenas Admin pode acessar (validado no Controller).
/// </summary>
public class GetAuditEventByIdUseCase
{
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly IMapper _mapper;

    public GetAuditEventByIdUseCase(
        IAuditEventRepository auditEventRepository,
        IMapper mapper)
    {
        _auditEventRepository = auditEventRepository;
        _mapper = mapper;
    }

    public async Task<AuditEventDto> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = await _auditEventRepository.GetByIdAsync(id, cancellationToken);

        if (auditEvent is null)
        {
            throw new BusinessRuleException(
                "AUDIT_EVENT_NOT_FOUND",
                $"Evento de auditoria com ID {id} não encontrado."
            );
        }

        return _mapper.Map<AuditEventDto>(auditEvent);
    }
}
