using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Services;
using L2SLedger.Application.UseCases.Adjustments;
using L2SLedger.Application.UseCases.Auth;
using L2SLedger.Application.UseCases.Categories;
using L2SLedger.Application.UseCases.Periods;
using L2SLedger.Application.UseCases.Transaction;
using L2SLedger.Application.Validators.Categories;
using L2SLedger.Infrastructure.Identity;
using L2SLedger.Infrastructure.Persistence.Repositories;
using L2SLedger.Infrastructure.Repositories;
using L2SLedger.Infrastructure.Services;
using FluentValidation;
using Polly;
using Polly.Extensions.Http;

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
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IFinancialPeriodRepository, FinancialPeriodRepository>();
        services.AddScoped<IAdjustmentRepository, AdjustmentRepository>();

        return services;
    }

    /// <summary>
    /// Registra serviços de aplicação.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
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
    /// Registra use cases de autenticação.
    /// </summary>
    public static IServiceCollection AddAuthUseCases(this IServiceCollection services)
    {
        services.AddScoped<FirebaseLoginUseCase>();

        return services;
    }

    /// <summary>
    /// Registra use cases de transações.
    /// </summary>
    public static IServiceCollection AddTransactionUseCases(this IServiceCollection services)
    {
        services.AddScoped<CreateTransactionUseCase>();
        services.AddScoped<UpdateTransactionUseCase>();
        services.AddScoped<GetTransactionsUseCase>();
        services.AddScoped<GetTransactionByIdUseCase>();
        services.AddScoped<DeleteTransactionUseCase>();

        return services;
    }

    /// <summary>
    /// Registra use cases de períodos financeiros.
    /// ADR-015: Períodos financeiros garantem imutabilidade de transações.
    /// </summary>
    public static IServiceCollection AddPeriodUseCases(this IServiceCollection services)
    {
        services.AddScoped<CreateFinancialPeriodUseCase>();
        services.AddScoped<ClosePeriodUseCase>();
        services.AddScoped<ReopenPeriodUseCase>();
        services.AddScoped<GetFinancialPeriodsUseCase>();
        services.AddScoped<GetFinancialPeriodByIdUseCase>();
        services.AddScoped<EnsurePeriodExistsAndOpenUseCase>();
        services.AddScoped<IPeriodBalanceService, PeriodBalanceService>();

        return services;
    }

    /// <summary>
    /// Registra use cases de ajustes pós-fechamento.
    /// ADR-015: Ajustes criam novos lançamentos, não alteram originais.
    /// </summary>
    public static IServiceCollection AddAdjustmentUseCases(this IServiceCollection services)
    {
        services.AddScoped<CreateAdjustmentUseCase>();
        services.AddScoped<GetAdjustmentsUseCase>();
        services.AddScoped<GetAdjustmentByIdUseCase>();
        services.AddScoped<DeleteAdjustmentUseCase>();

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
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Configurar HttpClient para FirebaseAuthenticationService com Polly
        services.AddHttpClient<IFirebaseAuthenticationService, FirebaseAuthenticationService>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    /// <summary>
    /// Política de retry para requisições HTTP transientes.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    /// <summary>
    /// Política de circuit breaker para requisições HTTP.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));
    }
}
