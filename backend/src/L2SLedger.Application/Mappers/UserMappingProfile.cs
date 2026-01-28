using AutoMapper;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Mappers;

/// <summary>
/// Profile de mapeamento AutoMapper para User.
/// Conforme ADR-020: Mapeamento entre Domain e Application.
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.ToList()));

        CreateMap<User, UserSummaryDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.ToList()));

        CreateMap<User, UserDetailDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.ToList()))
            .ForMember(dest => dest.LastLoginAt, opt => opt.Ignore());
    }
}
