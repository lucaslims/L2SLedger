using L2SLedger.API.Configuration;
using Serilog;

// Configurar Serilog (ADR-006, ADR-013)
ObservabilityExtensions.ConfigureSerilog();

try
{
    Log.Information("Iniciando L2SLedger API");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog como logger principal
    builder.AddSerilogConfiguration();

    // Configurar Firebase Admin SDK (ADR-001)
    builder.Services.AddFirebaseConfiguration(builder.Configuration);

    // Configurar Entity Framework Core com PostgreSQL (ADR-034, ADR-035)
    builder.Services.AddDatabaseConfiguration(builder.Configuration, builder.Environment);

    // Configurar AutoMapper (ADR-020)
    builder.Services.AddMappingConfiguration();

    // Registrar repositórios, serviços, use cases e validadores
    builder.Services.AddRepositories();
    builder.Services.AddApplicationServices();
    builder.Services.AddCategoryUseCases();
    builder.Services.AddAuthUseCases();
    builder.Services.AddTransactionUseCases();
    builder.Services.AddPeriodUseCases();
    builder.Services.AddAdjustmentUseCases();
    builder.Services.AddBalanceAndReportUseCases();
    builder.Services.AddExportUseCases();
    builder.Services.AddUserUseCases();
    builder.Services.AddAuditServices();
    builder.Services.AddValidators();
    builder.Services.AddInfrastructureServices();

    // Health Checks e Métricas (ADR-006)
    builder.Services.AddHealthCheckConfiguration();
    builder.Services.AddMetricsConfiguration();

    // Configurar Controllers e exception handling
    builder.Services.AddControllersConfiguration();

    // Configurar autenticação e autorização (ADR-001, ADR-002, ADR-004)
    builder.Services.AddCookieAuthenticationConfiguration();

    // Configurar Data Protection para persistir chaves de criptografia (ADR-004)
    builder.Services.AddDataProtectionConfiguration(builder.Environment);

    // Configurar Swagger/OpenAPI
    builder.Services.AddSwaggerConfiguration();

    // Configurar CORS (ADR-018)
    builder.Services.AddCorsConfiguration(builder.Configuration);

    var app = builder.Build();

    // Aplicar migrations automaticamente em Development (ADR-035)
    await app.ApplyMigrationsAsync();

    // Executar seed de dados padrão em Development (ADR-029)
    await app.SeedDatabaseAsync();

    // Configurar pipeline HTTP (exception handler, swagger, status codes, cors, auth)
    app.UseApiConfiguration();

    // Configurar observabilidade (Correlation ID + Serilog request logging)
    app.UseObservabilityConfiguration();

    // Mapear endpoints de Health Checks e Métricas (ADR-006)
    app.MapHealthCheckEndpoints();
    app.MapMetricsEndpoint();

    Log.Information("L2SLedger API iniciada com sucesso");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha ao iniciar L2SLedger API");
}
finally
{
    Log.CloseAndFlush();
}

// Partial class necessária para WebApplicationFactory em testes de integração
public partial class Program { }
