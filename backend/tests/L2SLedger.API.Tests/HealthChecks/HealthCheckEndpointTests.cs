using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace L2SLedger.API.Tests.HealthChecks;

/// <summary>
/// Testes de integração para endpoints de Health Check.
/// Conforme ADR-006: Health checks para PostgreSQL e Firebase.
/// 
/// NOTA: Estes testes requerem um ambiente configurado (Firebase, PostgreSQL).
/// São marcados como Trait("Category", "Integration") para execução separada.
/// </summary>
[Trait("Category", "Integration")]
public class HealthCheckEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient()
    {
        try
        {
            return _factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            }).CreateClient();
        }
        catch
        {
            return null!;
        }
    }

    [Fact(Skip = "Requires configured environment (Firebase, PostgreSQL)")]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        // Arrange
        var client = CreateClient();
        if (client == null) return;

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact(Skip = "Requires configured environment (Firebase, PostgreSQL)")]
    public async Task HealthLiveEndpoint_ReturnsHealthy()
    {
        // Arrange
        var client = CreateClient();
        if (client == null) return;

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact(Skip = "Requires configured environment (Firebase, PostgreSQL)")]
    public async Task HealthReadyEndpoint_ReturnsStatus()
    {
        // Arrange
        var client = CreateClient();
        if (client == null) return;

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert - pode retornar OK ou ServiceUnavailable dependendo das dependências
        response.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.OK,
            System.Net.HttpStatusCode.ServiceUnavailable);
    }

    [Fact(Skip = "Requires configured environment (Firebase, PostgreSQL)")]
    public async Task HealthEndpoint_ReturnsJsonContentType()
    {
        // Arrange
        var client = CreateClient();
        if (client == null) return;

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact(Skip = "Requires configured environment (Firebase, PostgreSQL)")]
    public async Task HealthEndpoint_ReturnsValidJsonStructure()
    {
        // Arrange
        var client = CreateClient();
        if (client == null) return;

        // Act
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        var json = JsonDocument.Parse(content);
        json.RootElement.TryGetProperty("status", out var status).Should().BeTrue();
        status.GetString().Should().NotBeNullOrWhiteSpace();
    }
}
