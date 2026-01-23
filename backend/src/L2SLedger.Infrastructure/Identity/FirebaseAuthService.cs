using FirebaseAdmin.Auth;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.Identity;

/// <summary>
/// Implementação do serviço de autenticação Firebase.
/// Conforme ADR-001, ADR-002 e ADR-007 (resiliência).
/// </summary>
public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly ILogger<FirebaseAuthService> _logger;
    private readonly TimeSpan _tokenValidationTimeout = TimeSpan.FromSeconds(5);

    public FirebaseAuthService(ILogger<FirebaseAuthService> logger)
    {
        _logger = logger;
    }

    public async Task<FirebaseUserData> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validando token Firebase");

            // Criar timeout para validação (ADR-007: resiliência)
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_tokenValidationTimeout);

            // Validar token com Firebase Admin SDK
            var decodedToken = await FirebaseAuth.DefaultInstance
                .VerifyIdTokenAsync(idToken, cts.Token);

            _logger.LogInformation("Token Firebase validado com sucesso para UID {Uid}", decodedToken.Uid);

            // Extrair email verified
            var emailVerified = decodedToken.Claims.TryGetValue("email_verified", out var emailVerifiedClaim)
                && emailVerifiedClaim is bool verified && verified;

            // Extrair nome (pode vir de diferentes claims)
            string? displayName = null;
            if (decodedToken.Claims.TryGetValue("name", out var nameClaim))
            {
                displayName = nameClaim?.ToString();
            }

            // Extrair email
            var email = decodedToken.Claims.TryGetValue("email", out var emailClaim)
                ? emailClaim?.ToString()
                : null;

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new AuthenticationException(
                    "AUTH_INVALID_TOKEN",
                    "Token Firebase não contém email válido");
            }

            return new FirebaseUserData
            {
                Uid = decodedToken.Uid,
                Email = email!,
                DisplayName = displayName,
                EmailVerified = emailVerified
            };
        }
        catch (FirebaseAuthException ex)
        {
            _logger.LogWarning(ex, "Token Firebase inválido: {Reason}", ex.Message);
            
            throw new AuthenticationException(
                "AUTH_INVALID_TOKEN",
                "Token de autenticação inválido ou expirado",
                ex);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError("Timeout ao validar token Firebase");
            
            throw new AuthenticationException(
                "AUTH_VALIDATION_TIMEOUT",
                "Timeout ao validar token de autenticação");
        }
    }
}
