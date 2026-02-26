using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
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

        if (string.IsNullOrWhiteSpace(firebaseCredentialPath))
        {
            throw new InvalidOperationException(
                "Firebase:CredentialPath não está configurado. " +
                "Defina a variável de ambiente FIREBASE_CREDENTIAL_PATH com o caminho para o arquivo de credenciais.");
        }

        if (!File.Exists(firebaseCredentialPath))
        {
            throw new InvalidOperationException(
                $"Firebase credential file não encontrado em '{firebaseCredentialPath}'. " +
                "Verifique se o volume está montado corretamente no container e se o arquivo existe no host.");
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(firebaseCredentialPath)
        });
        Log.Information("Firebase Admin SDK inicializado com sucesso a partir de '{Path}'", firebaseCredentialPath);

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

    /// <summary>
    /// Configura Data Protection para persistir chaves de criptografia.
    /// Garante que cookies permanecem válidos após reinício da aplicação.
    /// Conforme ADR-004 (Segurança de Cookies).
    /// </summary>
    public static IServiceCollection AddDataProtectionConfiguration(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        var keysPath = environment.IsDevelopment()
            ? Path.Combine(Directory.GetCurrentDirectory(), "keys")
            : "/app/keys";

        var keysDirectory = new DirectoryInfo(keysPath);
        if (!keysDirectory.Exists)
        {
            keysDirectory.Create();
            Log.Information("Diretório de chaves Data Protection criado: {KeysPath}", keysPath);
        }

        services.AddDataProtection()
            .PersistKeysToFileSystem(keysDirectory)
            .SetApplicationName("L2SLedger");

        Log.Information("Data Protection configurado com persistência em: {KeysPath}", keysPath);

        return services;
    }
}
