using L2SLedger.API.Middleware;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de API (CORS, Swagger, Controllers, Exception Handling).
/// Conforme ADR-018 (CORS), ADR-021 (Modelo de erros).
/// </summary>
public static class ApiExtensions
{
    /// <summary>
    /// Configura CORS para permitir frontend.
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? new[] { "http://localhost:3000" };

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();  // Necessário para cookies
            });
        });

        return services;
    }

    /// <summary>
    /// Configura Swagger/OpenAPI.
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    /// <summary>
    /// Configura Controllers e exception handling global.
    /// </summary>
    public static IServiceCollection AddControllersConfiguration(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Configura pipeline de middleware da API.
    /// </summary>
    public static WebApplication UseApiConfiguration(this WebApplication app)
    {
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

        // Configurar CORS
        app.UseCors("AllowFrontend");

        // Usar autenticação nativa do ASP.NET Core (ADR-002, ADR-004)
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
