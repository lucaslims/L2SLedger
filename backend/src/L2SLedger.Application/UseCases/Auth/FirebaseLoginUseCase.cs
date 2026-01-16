using FluentValidation;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Auth;

/// <summary>
/// Use case para login direto no Firebase com email e senha.
/// Usado apenas para testes em ambiente de desenvolvimento.
/// </summary>
public class FirebaseLoginUseCase
{
    private readonly IFirebaseAuthenticationService _firebaseAuthService;
    private readonly IValidator<FirebaseLoginRequest> _validator;
    private readonly ILogger<FirebaseLoginUseCase> _logger;

    public FirebaseLoginUseCase(
        IFirebaseAuthenticationService firebaseAuthService,
        IValidator<FirebaseLoginRequest> validator,
        ILogger<FirebaseLoginUseCase> logger)
    {
        _firebaseAuthService = firebaseAuthService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<FirebaseLoginResponse> ExecuteAsync(
        FirebaseLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        try
        {
            // 2. Chamar Firebase Authentication REST API
            var response = await _firebaseAuthService.SignInWithEmailPasswordAsync(
                request.Email,
                request.Password,
                cancellationToken);

            // 3. Log informativo (não logar senha!)
            _logger.LogInformation(
                "Firebase direct login successful for email: {Email}",
                request.Email);

            return response;
        }
        catch (AuthenticationException)
        {
            // Re-throw authentication exceptions
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Firebase direct login failed for email: {Email}",
                request.Email);
            
            throw new AuthenticationException(
                "AUTH_INVALID_CREDENTIALS",
                "Email ou senha inválidos");
        }
    }
}
