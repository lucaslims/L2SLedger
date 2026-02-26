using L2SLedger.API.Contracts;
using L2SLedger.API.Controllers;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;
using AppAuth = L2SLedger.Application.Interfaces.IAuthenticationService;
using AspNetAuth = Microsoft.AspNetCore.Authentication.IAuthenticationService;

namespace L2SLedger.API.Tests.Controllers;

public class AuthControllerFirebaseLoginTests
{
    private readonly Mock<AppAuth> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly AuthController _controller;

    public AuthControllerFirebaseLoginTests()
    {
        _authServiceMock = new Mock<AppAuth>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _envMock = new Mock<IWebHostEnvironment>();

        _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task FirebaseLogin_InProduction_ReturnsNotFound()
    {
        // Arrange
        var request = new FirebaseLoginRequest("teste@exemplo.com", "senha123");
        var mockUseCase = new Mock<L2SLedger.Application.UseCases.Auth.FirebaseLoginUseCase>(
            Mock.Of<IFirebaseAuthenticationService>(),
            Mock.Of<FluentValidation.IValidator<FirebaseLoginRequest>>(),
            Mock.Of<ILogger<L2SLedger.Application.UseCases.Auth.FirebaseLoginUseCase>>());

        _envMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        var result = await _controller.FirebaseLogin(
            request,
            mockUseCase.Object,
            _envMock.Object,
            CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void FirebaseLogin_InDevelopment_EndpointIsAccessible()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns("Development");

        // Assert - Verificar que o endpoint existe no controller
        var method = typeof(AuthController).GetMethod("FirebaseLogin");
        Assert.NotNull(method);
        Assert.Equal("FirebaseLogin", method.Name);
    }

    [Fact]
    public void FirebaseLogin_HasCorrectAttributes()
    {
        // Arrange & Act
        var method = typeof(AuthController).GetMethod("FirebaseLogin");
        var attributes = method?.GetCustomAttributes(false) ?? [];

        // Assert
        Assert.NotNull(method);
        Assert.Contains(attributes, attr => attr.GetType().Name == "HttpPostAttribute");
        Assert.Contains(attributes, attr => attr.GetType().Name == "AllowAnonymousAttribute");
    }
}

/// <summary>
/// Testes para o endpoint POST /auth/refresh (ADR-045).
/// </summary>
public class AuthControllerRefreshTests
{
    private readonly Mock<AppAuth> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly Mock<IFirebaseAuthService> _firebaseAuthServiceMock;

    public AuthControllerRefreshTests()
    {
        _authServiceMock = new Mock<AppAuth>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _firebaseAuthServiceMock = new Mock<IFirebaseAuthService>();
    }

    private AuthController CreateController(
        string? authorizationHeader = null,
        ClaimsPrincipal? user = null)
    {
        var httpContext = new DefaultHttpContext();

        // Configurar header Authorization
        if (authorizationHeader != null)
            httpContext.Request.Headers.Authorization = authorizationHeader;

        // Configurar ClaimsPrincipal (uso backend session)
        if (user != null)
            httpContext.User = user;

        // Configurar IAuthenticationService no ServiceProvider para SignInAsync
        var authServiceMockHttp = new Mock<AspNetAuth>(MockBehavior.Strict);
        authServiceMockHttp
            .Setup(s => s.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton<IAuthenticationHandler>(Mock.Of<IAuthenticationHandler>());

        // Mock do IAuthenticationService do ASP.NET (diferente do Application)
        var aspNetAuthMock = new Mock<AspNetAuth>();
        aspNetAuthMock
            .Setup(s => s.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(aspNetAuthMock.Object);

        httpContext.RequestServices = services.BuildServiceProvider();

        var controller = new AuthController(_authServiceMock.Object, _loggerMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        return controller;
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(string userId = "user-123")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, "usuario@teste.com"),
            new(ClaimTypes.Name, "Usuário Teste"),
            new(ClaimTypes.Role, "Leitura")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    // ── Testes de Atributos (ADR-045) ────────────────────────────────

    [Fact]
    public void Refresh_Method_HasHttpPostAttribute()
    {
        var method = typeof(AuthController).GetMethod("Refresh");
        var attributes = method?.GetCustomAttributes(false) ?? [];
        Assert.Contains(attributes, attr => attr.GetType().Name == "HttpPostAttribute");
    }

    [Fact]
    public void Refresh_Method_HasAllowAnonymousAttribute()
    {
        // Endpoint requer Bearer token, não cookie — logo AllowAnonymous é correto
        var method = typeof(AuthController).GetMethod("Refresh");
        var attributes = method?.GetCustomAttributes(false) ?? [];
        Assert.Contains(attributes, attr => attr.GetType().Name == "AllowAnonymousAttribute");
    }

    [Fact]
    public void AuthController_CookieExpiration_IsOneHour()
    {
        // Validar que TTL do cookie é 1 hora conforme ADR-045
        var field = typeof(AuthController)
            .GetField("CookieExpiration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.NotNull(field);
        var value = (TimeSpan)field!.GetValue(null)!;
        Assert.Equal(TimeSpan.FromHours(1), value);
    }

    // ── Testes de Comportamento ───────────────────────────────────────

    [Fact]
    public async Task Refresh_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController(authorizationHeader: null);

        // Act
        var result = await controller.Refresh(_firebaseAuthServiceMock.Object, CancellationToken.None);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        var body = Assert.IsType<ErrorResponse>(unauthorized.Value);
        Assert.Equal(ErrorCodes.AUTH_INVALID_TOKEN, body.Error.Code);
    }

    [Fact]
    public async Task Refresh_WithNonBearerAuthorizationHeader_ReturnsUnauthorized()
    {
        // Arrange
        var controller = CreateController(authorizationHeader: "Basic dXNlcjpwYXNz");

        // Act
        var result = await controller.Refresh(_firebaseAuthServiceMock.Object, CancellationToken.None);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        var body = Assert.IsType<ErrorResponse>(unauthorized.Value);
        Assert.Equal(ErrorCodes.AUTH_INVALID_TOKEN, body.Error.Code);
    }

    [Fact]
    public async Task Refresh_WithInvalidFirebaseToken_ReturnsUnauthorized()
    {
        // Arrange
        _firebaseAuthServiceMock
            .Setup(f => f.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AuthenticationException(ErrorCodes.AUTH_INVALID_TOKEN, "Token Firebase inválido"));

        var controller = CreateController(authorizationHeader: "Bearer token-invalido");

        // Act
        var result = await controller.Refresh(_firebaseAuthServiceMock.Object, CancellationToken.None);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        var body = Assert.IsType<ErrorResponse>(unauthorized.Value);
        Assert.Equal(ErrorCodes.AUTH_INVALID_TOKEN, body.Error.Code);
    }

    [Fact]
    public async Task Refresh_WithValidTokenButNoBackendSession_ReturnsUnauthorized()
    {
        // Arrange — Firebase token válido mas sem sessão backend (sem NameIdentifier claim)
        _firebaseAuthServiceMock
            .Setup(f => f.ValidateTokenAsync("bearer-token-valido", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FirebaseUserData
            {
                Uid = "firebase-uid",
                Email = "user@test.com",
                EmailVerified = true,
                DisplayName = "Teste"
            });

        // Usuário sem claims de sessão backend
        var controller = CreateController(authorizationHeader: "Bearer bearer-token-valido");

        // Act
        var result = await controller.Refresh(_firebaseAuthServiceMock.Object, CancellationToken.None);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        var body = Assert.IsType<ErrorResponse>(unauthorized.Value);
        Assert.Equal(ErrorCodes.AUTH_INVALID_TOKEN, body.Error.Code);
    }

    [Fact]
    public async Task Refresh_WithValidTokenAndActiveBackendSession_ReturnsOk()
    {
        // Arrange — Firebase token válido + sessão backend ativa
        _firebaseAuthServiceMock
            .Setup(f => f.ValidateTokenAsync("valid-firebase-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FirebaseUserData
            {
                Uid = "firebase-uid",
                Email = "user@test.com",
                EmailVerified = true,
                DisplayName = "Usuário Teste"
            });

        var authenticatedUser = CreateAuthenticatedUser("user-guid-123");
        var controller = CreateController(
            authorizationHeader: "Bearer valid-firebase-token",
            user: authenticatedUser);

        // Act
        var result = await controller.Refresh(_firebaseAuthServiceMock.Object, CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);
    }
}
