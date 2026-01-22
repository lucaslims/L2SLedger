using FluentAssertions;
using L2SLedger.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace L2SLedger.Infrastructure.Tests.HealthChecks;

/// <summary>
/// Testes unitários para FirebaseHealthCheck.
/// Conforme ADR-006: Health checks para dependências externas.
/// </summary>
public class FirebaseHealthCheckTests
{
    private readonly Mock<ILogger<FirebaseHealthCheck>> _loggerMock;

    public FirebaseHealthCheckTests()
    {
        _loggerMock = new Mock<ILogger<FirebaseHealthCheck>>();
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task CheckHealthAsync_ReturnsHealthy_WhenFirebaseResponds(HttpStatusCode statusCode)
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var healthCheck = new FirebaseHealthCheck(httpClient, _loggerMock.Object);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("firebase", healthCheck, null, null)
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("acessível");
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsDegraded_WhenUnexpectedStatus()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var healthCheck = new FirebaseHealthCheck(httpClient, _loggerMock.Object);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("firebase", healthCheck, null, null)
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("InternalServerError");
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsUnhealthy_WhenTimeout()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        var httpClient = new HttpClient(handlerMock.Object);
        var healthCheck = new FirebaseHealthCheck(httpClient, _loggerMock.Object);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("firebase", healthCheck, null, null)
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("timeout");
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsUnhealthy_WhenConnectionFails()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(handlerMock.Object);
        var healthCheck = new FirebaseHealthCheck(httpClient, _loggerMock.Object);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("firebase", healthCheck, null, null)
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("conectar");
    }
}
