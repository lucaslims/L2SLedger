using System.Net;
using System.Text.Json;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Domain.Exceptions;
using L2SLedger.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace L2SLedger.Infrastructure.Tests.Identity;

public class FirebaseAuthenticationServiceTests
{
    private readonly Mock<ILogger<FirebaseAuthenticationService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private const string TestWebApiKey = "test-api-key-123";
    private const string TestCredentialPath = "test-credentials.json";

    public FirebaseAuthenticationServiceTests()
    {
        _loggerMock = new Mock<ILogger<FirebaseAuthenticationService>>();
        _configurationMock = new Mock<IConfiguration>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        // Setup configuração padrão
        _configurationMock.Setup(c => c["Firebase:WebApiKey"]).Returns(TestWebApiKey);
        _configurationMock.Setup(c => c["Firebase:CredentialPath"]).Returns(TestCredentialPath);

        // Criar arquivo temporário de credenciais para testes
        var credentialJson = JsonSerializer.Serialize(new
        {
            project_id = "test-project",
            client_email = "test@test.iam.gserviceaccount.com"
        });
        File.WriteAllText(TestCredentialPath, credentialJson);
    }

    [Fact]
    public async Task SignInWithEmailPasswordAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var expectedResponse = new
        {
            idToken = "test-id-token",
            refreshToken = "test-refresh-token",
            expiresIn = "3600",
            localId = "test-local-id",
            email = "teste@exemplo.com",
            registered = true
        };

        var responseContent = new StringContent(
            JsonSerializer.Serialize(expectedResponse),
            System.Text.Encoding.UTF8,
            "application/json");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = responseContent
            });

        var service = new FirebaseAuthenticationService(
            _httpClient,
            _configurationMock.Object,
            _loggerMock.Object);

        // Act
        var result = await service.SignInWithEmailPasswordAsync(
            "teste@exemplo.com",
            "senha123",
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-id-token", result.IdToken);
        Assert.Equal("test-refresh-token", result.RefreshToken);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.Equal("teste@exemplo.com", result.Email);
        Assert.True(result.Registered);

        // Cleanup
        File.Delete(TestCredentialPath);
    }

    [Fact]
    public async Task SignInWithEmailPasswordAsync_InvalidCredentials_ThrowsAuthenticationException()
    {
        // Arrange
        var errorResponse = new
        {
            error = new
            {
                code = 400,
                message = "INVALID_PASSWORD"
            }
        };

        var responseContent = new StringContent(
            JsonSerializer.Serialize(errorResponse),
            System.Text.Encoding.UTF8,
            "application/json");

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = responseContent
            });

        var service = new FirebaseAuthenticationService(
            _httpClient,
            _configurationMock.Object,
            _loggerMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AuthenticationException>(
            () => service.SignInWithEmailPasswordAsync(
                "teste@exemplo.com",
                "senhaerrada",
                CancellationToken.None));

        Assert.Equal("AUTH_FIREBASE_ERROR", exception.Code);

        // Cleanup
        File.Delete(TestCredentialPath);
    }

    [Fact]
    public async Task SignInWithEmailPasswordAsync_FirebaseUnavailable_ThrowsAuthenticationException()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var service = new FirebaseAuthenticationService(
            _httpClient,
            _configurationMock.Object,
            _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.SignInWithEmailPasswordAsync(
                "teste@exemplo.com",
                "senha123",
                CancellationToken.None));

        // Cleanup
        File.Delete(TestCredentialPath);
    }

    [Fact]
    public void Constructor_WithoutWebApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        _configurationMock.Setup(c => c["Firebase:WebApiKey"]).Returns((string?)null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => new FirebaseAuthenticationService(
                _httpClient,
                _configurationMock.Object,
                _loggerMock.Object));

        // Cleanup
        if (File.Exists(TestCredentialPath))
            File.Delete(TestCredentialPath);
    }
}
