using System.Text.Json;
using AutoMapper;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.Mappers;

/// <summary>
/// Perfil AutoMapper para mapeamentos da entidade FinancialPeriod.
/// Mapeia entre entidades de domínio e DTOs seguindo princípios de Clean Architecture.
/// </summary>
public class FinancialPeriodMappingProfile : Profile
{
    public FinancialPeriodMappingProfile()
    {
        CreateMap<FinancialPeriod, FinancialPeriodDto>()
            .ForMember(dest => dest.PeriodName, opt => opt.MapFrom(src => src.GetPeriodName()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ClosedByUserName, opt => opt.MapFrom(src => 
                src.ClosedByUser != null ? src.ClosedByUser.DisplayName : null))
            .ForMember(dest => dest.ReopenedByUserName, opt => opt.MapFrom(src => 
                src.ReopenedByUser != null ? src.ReopenedByUser.DisplayName : null))
            .ForMember(dest => dest.BalanceSnapshot, opt => opt.MapFrom(src => 
                !string.IsNullOrEmpty(src.BalanceSnapshotJson) 
                    ? JsonSerializer.Deserialize<BalanceSnapshot>(src.BalanceSnapshotJson, (JsonSerializerOptions?)null) 
                    : null));
    }
}
