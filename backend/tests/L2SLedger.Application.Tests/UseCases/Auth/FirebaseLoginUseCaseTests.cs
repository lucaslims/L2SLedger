using FluentValidation;
using FluentValidation.Results;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Auth;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace L2SLedger.Application.Tests.UseCases.Auth;

public class FirebaseLoginUseCaseTests
{
    private readonly Mock<IFirebaseAuthenticationService> _firebaseAuthServiceMock;
    private readonly Mock<IValidator<FirebaseLoginRequest>> _validatorMock;
    private readonly Mock<ILogger<FirebaseLoginUseCase>> _loggerMock;
    private readonly FirebaseLoginUseCase _useCase;

    public FirebaseLoginUseCaseTests()
    {
        _firebaseAuthServiceMock = new Mock<IFirebaseAuthenticationService>();
        _validatorMock = new Mock<IValidator<FirebaseLoginRequest>>();
        _loggerMock = new Mock<ILogger<FirebaseLoginUseCase>>();

        _useCase = new FirebaseLoginUseCase(
            _firebaseAuthServiceMock.Object,
            _validatorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_ReturnsIdToken()
    {
        // Arrange
        var request = new FirebaseLoginRequest("teste@exemplo.com", "senha123");
        var expectedResponse = new FirebaseLoginResponse(
            "idToken123",
            "refreshToken123",
            3600,
            "localId123",
            "teste@exemplo.com",
            true
        );

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _firebaseAuthServiceMock
            .Setup(s => s.SignInWithEmailPasswordAsync(
                request.Email,
                request.Password,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _useCase.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.IdToken, result.IdToken);
        Assert.Equal(expectedResponse.Email, result.Email);
        Assert.Equal(expectedResponse.LocalId, result.LocalId);
        
        _firebaseAuthServiceMock.Verify(
            s => s.SignInWithEmailPasswordAsync(request.Email, request.Password, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEmail_ThrowsValidationException()
    {
        // Arrange
        var request = new FirebaseLoginRequest("email-invalido", "senha123");
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Email inválido")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.ExecuteAsync(request, CancellationToken.None));

        _firebaseAuthServiceMock.Verify(
            s => s.SignInWithEmailPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPassword_ThrowsAuthenticationException()
    {
        // Arrange
        var request = new FirebaseLoginRequest("teste@exemplo.com", "senhaerrada");

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _firebaseAuthServiceMock
            .Setup(s => s.SignInWithEmailPasswordAsync(
                request.Email,
                request.Password,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AuthenticationException("AUTH_FIREBASE_ERROR", "Credenciais inválidas"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AuthenticationException>(
            () => _useCase.ExecuteAsync(request, CancellationToken.None));

        Assert.Equal("AUTH_FIREBASE_ERROR", exception.Code);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyFields_ThrowsValidationException()
    {
        // Arrange
        var request = new FirebaseLoginRequest("", "");
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Email é obrigatório"),
            new ValidationFailure("Password", "Senha é obrigatória")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.ExecuteAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var request = new FirebaseLoginRequest("teste@exemplo.com", "senha123");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _firebaseAuthServiceMock
            .Setup(s => s.SignInWithEmailPasswordAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<AuthenticationException>(
            () => _useCase.ExecuteAsync(request, cts.Token));
    }
}
