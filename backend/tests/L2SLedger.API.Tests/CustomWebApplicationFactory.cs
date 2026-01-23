using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace L2SLedger.API.Tests;

/// <summary>
/// Custom WebApplicationFactory para testes de integração.
/// Configura o ambiente de teste isolando dependências externas como Firebase.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Remove serviços que dependem de recursos externos para testes
            // Neste caso, podemos adicionar mocks se necessário
        });
    }
}
