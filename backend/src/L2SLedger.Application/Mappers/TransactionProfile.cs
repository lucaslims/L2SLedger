using AutoMapper;
using L2SLedger.Application.DTOs.Transaction;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Mappers;

/// <summary>
/// Perfil do AutoMapper para Transaction.
/// </summary>
public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
    }
}
