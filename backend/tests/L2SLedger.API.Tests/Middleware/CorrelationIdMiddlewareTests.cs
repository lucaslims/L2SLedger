using FluentAssertions;
using L2SLedger.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace L2SLedger.API.Tests.Middleware;

/// <summary>
/// Testes unitários para CorrelationIdMiddleware.
/// Conforme ADR-006: Correlação de requisições via Correlation ID.
/// </summary>
public class CorrelationIdMiddlewareTests
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock;

    public CorrelationIdMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_GeneratesCorrelationId_WhenNotProvided()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.Headers.Should().ContainKey(CorrelationIdHeader);
        context.Response.Headers[CorrelationIdHeader].ToString().Should().NotBeNullOrWhiteSpace();
        context.Response.Headers[CorrelationIdHeader].ToString().Should().HaveLength(16);
    }

    [Fact]
    public async Task InvokeAsync_UsesExistingCorrelationId_WhenProvided()
    {
        // Arrange
        var existingCorrelationId = "existing-id-12345";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = existingCorrelationId;

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.Headers[CorrelationIdHeader].ToString().Should().Be(existingCorrelationId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task InvokeAsync_GeneratesNewId_WhenHeaderIsEmptyOrWhitespace(string? headerValue)
    {
        // Arrange
        var context = new DefaultHttpContext();
        if (headerValue != null)
        {
            context.Request.Headers[CorrelationIdHeader] = headerValue;
        }

        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.Headers[CorrelationIdHeader].ToString().Should().NotBeNullOrWhiteSpace();
        context.Response.Headers[CorrelationIdHeader].ToString().Should().HaveLength(16);
    }
}
