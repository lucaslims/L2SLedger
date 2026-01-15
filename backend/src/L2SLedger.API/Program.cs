using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using L2SLedger.API.Middleware;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.Mappers;
using L2SLedger.Application.UseCases.Auth;
using L2SLedger.Application.UseCases.Categories;
using L2SLedger.Application.Validators.Categories;
using L2SLedger.Infrastructure.Identity;
using L2SLedger.Infrastructure.Persistence;
using L2SLedger.Infrastructure.Persistence.Repositories;
using L2SLedger.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
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
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

    // Registrar serviços de aplicação
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

    // Registrar use cases de categorias
    builder.Services.AddScoped<CreateCategoryUseCase>();
    builder.Services.AddScoped<UpdateCategoryUseCase>();
    builder.Services.AddScoped<GetCategoriesUseCase>();
    builder.Services.AddScoped<GetCategoryByIdUseCase>();
    builder.Services.AddScoped<DeactivateCategoryUseCase>();

    // Registrar validadores FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();

    // Registrar serviços de infraestrutura
    builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

    // Configurar Controllers
    builder.Services.AddControllers();

    // Registrar exception handler global
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Configurar autenticação e autorização (ADR-002, ADR-004)
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "l2sledger-auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromHours(6);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            };
        });
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
    
    // Exception handler deve vir primeiro
    app.UseExceptionHandler();
    
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Status code pages para retornar erros padronizados em JSON
    app.UseStatusCodePages(async context =>
    {
        var response = context.HttpContext.Response;
        if (response.StatusCode == 401)
        {
            response.ContentType = "application/json";
            await response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "AUTH_UNAUTHORIZED",
                    message = "Usuário não autenticado",
                    timestamp = DateTime.UtcNow,
                    traceId = context.HttpContext.TraceIdentifier
                }
            });
        }
        else if (response.StatusCode == 403)
        {
            response.ContentType = "application/json";
            await response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "AUTH_FORBIDDEN",
                    message = "Acesso negado",
                    timestamp = DateTime.UtcNow,
                    traceId = context.HttpContext.TraceIdentifier
                }
            });
        }
    });

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
