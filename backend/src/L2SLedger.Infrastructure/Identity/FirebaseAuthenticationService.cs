using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.Identity;

/// <summary>
/// Implementação do serviço de autenticação direta no Firebase via REST API.
/// Usado apenas para testes em ambiente de desenvolvimento.
/// </summary>
public class FirebaseAuthenticationService : IFirebaseAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly string _webApiKey;
    private readonly ILogger<FirebaseAuthenticationService> _logger;

    public FirebaseAuthenticationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<FirebaseAuthenticationService> logger)
    {
        _httpClient = httpClient;
        
        // Extrair Web API Key da configuração
        var credentialPath = configuration["Firebase:CredentialPath"];
        if (!string.IsNullOrEmpty(credentialPath) && File.Exists(credentialPath))
        {
            // Ler o arquivo JSON de credenciais para logging/validação
            var credentialJson = File.ReadAllText(credentialPath);
            var credential = JsonSerializer.Deserialize<FirebaseCredential>(credentialJson);
            
            if (credential != null)
            {
                logger.LogInformation(
                    "FirebaseAuthenticationService initialized for project: {ProjectId}", 
                    credential.ProjectId);
            }
        }
        
        // Web API Key deve estar em configuração separada (não é secreto)
        _webApiKey = configuration["Firebase:WebApiKey"] 
            ?? throw new InvalidOperationException(
                "Firebase:WebApiKey not configured. Get it from Firebase Console > Project Settings > Web API Key");
        
        _logger = logger;
    }

    public async Task<FirebaseLoginResponse> SignInWithEmailPasswordAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_webApiKey}";

        var requestBody = new
        {
            email = email,
            password = password,
            returnSecureToken = true
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Firebase login failed. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            throw new AuthenticationException(
                ErrorCodes.AUTH_FIREBASE_ERROR,
                "Erro ao autenticar com Firebase");
        }

        var result = await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>(cancellationToken);
        
        if (result == null)
            throw new AuthenticationException(ErrorCodes.AUTH_FIREBASE_ERROR, "Resposta inválida do Firebase");

        return new FirebaseLoginResponse(
            result.IdToken,
            result.RefreshToken,
            int.Parse(result.ExpiresIn),
            result.LocalId,
            result.Email,
            result.Registered
        );
    }

    // DTOs internos
    private record FirebaseCredential(
        [property: JsonPropertyName("project_id")] string ProjectId,
        [property: JsonPropertyName("client_email")] string ClientEmail
    );

    private record FirebaseSignInResponse(
        string IdToken,
        string RefreshToken,
        string ExpiresIn,
        string LocalId,
        string Email,
        bool Registered
    );
}
