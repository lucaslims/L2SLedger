namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Serviço para obter informações do usuário autenticado.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Obtém o ID do usuário autenticado.
    /// </summary>
    /// <returns>ID do usuário.</returns>
    /// <exception cref="UnauthorizedAccessException">Quando o usuário não está autenticado.</exception>
    Guid GetUserId();

    /// <summary>
    /// Obtém o email do usuário autenticado.
    /// </summary>
    /// <returns>Email do usuário.</returns>
    string? GetUserEmail();
}
