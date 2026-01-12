using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using L2SLedger.API.Middleware;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Auth;
using L2SLedger.Infrastructure.Identity;
using L2SLedger.Infrastructure.Persistence;
using L2SLedger.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Configurar Serilog (ADR-013)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .WriteTo.File(
        formatter: new Serilog.Formatting.Compact.CompactJsonFormatter(), 
        path: "logs/l2sledger-.log", 
        rollingInterval: RollingInterval.Minute, 
        flushToDiskInterval: TimeSpan.FromDays(5), 
        retainedFileCountLimit: 15)
    .Enrich.WithProperty("Application", "L2SLedger.API")
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Iniciando L2SLedger API");

    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog como logger principal
    builder.Host.UseSerilog();

    // Configurar Firebase Admin SDK (ADR-001)
    var firebaseCredentialPath = builder.Configuration["Firebase:CredentialPath"];
    if (!string.IsNullOrEmpty(firebaseCredentialPath) && File.Exists(firebaseCredentialPath))
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(firebaseCredentialPath)
        });
        Log.Information("Firebase Admin SDK inicializado");
    }
    else
    {
        Log.Warning("Firebase credential não configurado ou arquivo não encontrado");
    }

    // Configurar Entity Framework Core com PostgreSQL (ADR-006)
    builder.Services.AddDbContext<L2SLedgerDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("L2SLedger.Infrastructure");
        });
        
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    // Configurar AutoMapper (ADR-020)
    builder.Services.AddAutoMapper(typeof(Program).Assembly, 
        typeof(AuthProfile).Assembly);

    // Registrar repositórios
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    // Registrar serviços de aplicação
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

    // Registrar serviços de infraestrutura
    builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

    // Configurar Controllers
    builder.Services.AddControllers();

    // Configurar autenticação e autorização
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();

    // Configurar OpenAPI/Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Configurar CORS (ADR-018)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                ?? new[] { "http://localhost:3000" };
            
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();  // Necessário para cookies
        });
    });

    var app = builder.Build();

    // Aplicar migrations automaticamente em Development
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<L2SLedgerDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Migrations aplicadas com sucesso");
    }

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Usar Serilog para logs HTTP
    app.UseSerilogRequestLogging();

    // Configurar CORS
    app.UseCors("AllowFrontend");

    // Middleware de autenticação customizado (ADR-002, ADR-004)
    app.UseMiddleware<AuthenticationMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

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
