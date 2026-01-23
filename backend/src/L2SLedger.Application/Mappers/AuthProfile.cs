using AutoMapper;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Mappers;

/// <summary>
/// Profile do AutoMapper para autenticação.
/// Conforme ADR-020 (Clean Architecture).
/// </summary>
public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles));
    }
}
