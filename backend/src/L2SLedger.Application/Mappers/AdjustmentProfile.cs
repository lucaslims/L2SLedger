using AutoMapper;
using L2SLedger.Application.DTOs.Adjustments;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Mappers;

/// <summary>
/// Perfil do AutoMapper para Adjustment.
/// </summary>
public class AdjustmentProfile : Profile
{
    public AdjustmentProfile()
    {
        CreateMap<Adjustment, AdjustmentDto>()
            .ForMember(dest => dest.OriginalTransactionDescription, 
                opt => opt.MapFrom(src => src.OriginalTransaction != null ? src.OriginalTransaction.Description : string.Empty))
            .ForMember(dest => dest.TypeName, 
                opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.CreatedByUserName, 
                opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.DisplayName : string.Empty));
    }
}
