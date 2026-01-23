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

    /// <summary>
    /// Obtém o nome do usuário autenticado.
    /// </summary>
    /// <returns>Nome do usuário.</returns>
    string? GetUserName();

    /// <summary>
    /// Verifica se o usuário autenticado possui uma determinada role.
    /// </summary>
    /// <param name="role">Nome da role a verificar.</param>
    /// <returns>True se o usuário possui a role, False caso contrário.</returns>
    bool IsInRole(string role);
}
