using FluentValidation;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Audit;
using L2SLedger.Application.Validators.Audit;
using L2SLedger.Infrastructure.Persistence.Repositories;
using L2SLedger.Infrastructure.Services;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Extensões para configuração de serviços de auditoria.
/// Conforme ADR-014 (Auditoria Financeira) e ADR-019 (Auditoria de Acessos).
/// </summary>
public static class AuditExtensions
{
    /// <summary>
    /// Adiciona serviços de auditoria ao container de DI.
    /// </summary>
    public static IServiceCollection AddAuditServices(this IServiceCollection services)
    {
        // Repositório
        services.AddScoped<IAuditEventRepository, AuditEventRepository>();

        // Serviço de auditoria
        services.AddScoped<IAuditService, AuditService>();

        // Use Cases
        services.AddScoped<GetAuditEventsUseCase>();
        services.AddScoped<GetAuditEventByIdUseCase>();

        // Validators
        services.AddScoped<IValidator<GetAuditEventsRequest>, GetAuditEventsRequestValidator>();

        return services;
    }
}
