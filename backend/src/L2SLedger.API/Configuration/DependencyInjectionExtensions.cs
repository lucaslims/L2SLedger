using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Auth;
using L2SLedger.Application.UseCases.Categories;
using L2SLedger.Application.Validators.Categories;
using L2SLedger.Infrastructure.Identity;
using L2SLedger.Infrastructure.Persistence.Repositories;
using L2SLedger.Infrastructure.Repositories;
using FluentValidation;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de injeção de dependência (repositórios, services, use cases).
/// Conforme ADR-020 (Clean Architecture).
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registra todos os repositórios da camada de infraestrutura.
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }

    /// <summary>
    /// Registra serviços de aplicação.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }

    /// <summary>
    /// Registra use cases de categorias.
    /// </summary>
    public static IServiceCollection AddCategoryUseCases(this IServiceCollection services)
    {
        services.AddScoped<CreateCategoryUseCase>();
        services.AddScoped<UpdateCategoryUseCase>();
        services.AddScoped<GetCategoriesUseCase>();
        services.AddScoped<GetCategoryByIdUseCase>();
        services.AddScoped<DeactivateCategoryUseCase>();

        return services;
    }

    /// <summary>
    /// Registra validadores FluentValidation.
    /// </summary>
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();

        return services;
    }

    /// <summary>
    /// Registra serviços de infraestrutura (Firebase, etc).
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

        return services;
    }
}
