using AutoMapper;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Mappers;

/// <summary>
/// Perfil de mapeamento para eventos de auditoria.
/// </summary>
public class AuditProfile : Profile
{
    public AuditProfile()
    {
        CreateMap<AuditEvent, AuditEventDto>()
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => (int)src.EventType))
            .ForMember(dest => dest.EventTypeName, opt => opt.MapFrom(src => src.EventType.ToString()))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => (int)src.Source))
            .ForMember(dest => dest.SourceName, opt => opt.MapFrom(src => src.Source.ToString()));
    }
}
