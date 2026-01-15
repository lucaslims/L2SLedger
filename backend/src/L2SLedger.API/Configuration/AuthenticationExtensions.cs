using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de autenticação e autorização.
/// Conforme ADR-001 (Firebase), ADR-002 (Fluxo completo), ADR-004 (Cookies seguros).
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Configura Firebase Admin SDK.
    /// </summary>
    public static IServiceCollection AddFirebaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var firebaseCredentialPath = configuration["Firebase:CredentialPath"];
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

        return services;
    }

    /// <summary>
    /// Configura Cookie Authentication com segurança (HttpOnly, Secure, SameSite).
    /// </summary>
    public static IServiceCollection AddCookieAuthenticationConfiguration(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
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

        services.AddAuthorization();

        return services;
    }
}
