namespace L2SLedger.Application.DTOs.Auth;

/// <summary>
/// Response do login bem-sucedido.
/// </summary>
public record LoginResponse
{
    /// <summary>
    /// Dados do usuário autenticado.
    /// </summary>
    public required UserDto User { get; init; }

    /// <summary>
    /// Mensagem de sucesso.
    /// </summary>
    public string Message { get; init; } = "Login realizado com sucesso";
}
