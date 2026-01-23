using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.HealthChecks;

/// <summary>
/// Health Check para verificar disponibilidade do Firebase Authentication.
/// Conforme ADR-006: Health checks para dependências externas.
/// </summary>
public class FirebaseHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FirebaseHealthCheck> _logger;
    private const string FirebaseHealthUrl = "https://identitytoolkit.googleapis.com/v1/projects";

    public FirebaseHealthCheck(
        HttpClient httpClient,
        ILogger<FirebaseHealthCheck> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, FirebaseHealthUrl);
            request.Headers.Add("Accept", "application/json");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5)); // Timeout de 5 segundos (ADR-007)

            var response = await _httpClient.SendAsync(request, cts.Token);

            // Firebase retorna 401/403 sem autenticação, mas isso significa que está respondendo
            if (response.IsSuccessStatusCode ||
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return HealthCheckResult.Healthy("Firebase Authentication API está acessível.");
            }

            _logger.LogWarning("Firebase retornou status inesperado: {StatusCode}", response.StatusCode);
            return HealthCheckResult.Degraded($"Firebase retornou status {response.StatusCode}");
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Timeout ao verificar Firebase");
            return HealthCheckResult.Unhealthy("Firebase Authentication API não respondeu dentro do timeout.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro ao verificar Firebase");
            return HealthCheckResult.Unhealthy("Não foi possível conectar ao Firebase Authentication API.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao verificar Firebase");
            return HealthCheckResult.Unhealthy("Erro inesperado ao verificar Firebase.", ex);
        }
    }
}
