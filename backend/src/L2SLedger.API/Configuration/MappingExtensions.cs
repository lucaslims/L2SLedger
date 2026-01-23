using L2SLedger.Application.Mappers;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de mapeamento (AutoMapper).
/// Conforme ADR-020 (Clean Architecture).
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Configura AutoMapper com profiles de mapeamento.
    /// </summary>
    public static IServiceCollection AddMappingConfiguration(this IServiceCollection services)
    {
        services.AddAutoMapper(
            typeof(Program).Assembly,
            typeof(AuthProfile).Assembly
        );

        return services;
    }
}
