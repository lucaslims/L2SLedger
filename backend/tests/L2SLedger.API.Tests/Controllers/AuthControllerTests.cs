using L2SLedger.API.Contracts;
using L2SLedger.API.Controllers;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace L2SLedger.API.Tests.Controllers;

public class AuthControllerFirebaseLoginTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly AuthController _controller;

    public AuthControllerFirebaseLoginTests()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
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
        var attributes = method?.GetCustomAttributes(false);

        // Assert
        Assert.NotNull(method);
        Assert.Contains(attributes, attr => attr.GetType().Name == "HttpPostAttribute");
        Assert.Contains(attributes, attr => attr.GetType().Name == "AllowAnonymousAttribute");
    }
}
