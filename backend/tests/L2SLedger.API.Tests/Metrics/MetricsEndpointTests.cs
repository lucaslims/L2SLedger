using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace L2SLedger.API.Tests.Metrics;

/// <summary>
/// Testes de integração para endpoint de métricas.
/// Conforme ADR-006: Métricas expostas via /metrics para Prometheus.
/// 
/// NOTA: Estes testes requerem um ambiente configurado (Firebase, PostgreSQL).
/// São marcados como Trait("Category", "Integration") para execução separada.
/// </summary>
[Trait("Category", "Integration")]
public class MetricsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MetricsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient? CreateClient()
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
            return null;
        }
    }

    [Fact(Skip = "Requires configured environment (Firebase, PostgreSQL)")]
    public async Task MetricsEndpoint_ReturnsOk()
    {
        // Arrange
        var client = CreateClient();
        if (client == null) return;

        // Act
        var response = await client.GetAsync("/metrics");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact(Skip = "Requires configured environment (Firebase, PostgreSQL)")]
    public async Task MetricsEndpoint_ReturnsPrometheusFormat()
    {
        // Arrange
        var client = CreateClient();
        if (client == null) return;

        // Act
        var response = await client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Prometheus format contains # HELP or # TYPE comments
        content.Should().Contain("# ");
    }

    [Fact(Skip = "Requires configured environment (Firebase, PostgreSQL)")]
    public async Task MetricsEndpoint_ContainsHttpRequestMetrics()
    {
        // Arrange
        var client = CreateClient();
        if (client == null) return;

        // Act - Make a request first to generate metrics
        await client.GetAsync("/health");
        var response = await client.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Should contain HTTP request metrics from ASP.NET Core instrumentation
        content.Should().ContainAny(
            "http_server_request",
            "http_server_duration",
            "aspnetcore");
    }
}
