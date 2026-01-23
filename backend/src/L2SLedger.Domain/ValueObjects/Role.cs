namespace L2SLedger.Domain.ValueObjects;

/// <summary>
/// Value Object para representar um papel/role do sistema.
/// Conforme ADR-016: Admin, Financeiro, Leitura.
/// </summary>
public sealed record Role
{
    public static readonly Role Admin = new("Admin");
    public static readonly Role Financeiro = new("Financeiro");
    public static readonly Role Leitura = new("Leitura");

    private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        Admin.Value,
        Financeiro.Value,
        Leitura.Value
    };

    public string Value { get; }

    private Role(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Cria uma instância de Role a partir de uma string.
    /// </summary>
    /// <param name="role">Nome do role.</param>
    /// <returns>Instância de Role.</returns>
    /// <exception cref="ArgumentException">Quando o role é inválido.</exception>
    public static Role FromString(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role não pode ser vazio.", nameof(role));

        if (!IsValid(role))
            throw new ArgumentException($"Role inválido: {role}. Valores permitidos: {string.Join(", ", ValidRoles)}");

        return new Role(role);
    }

    /// <summary>
    /// Verifica se um role é válido.
    /// </summary>
    /// <param name="role">Nome do role a verificar.</param>
    /// <returns>True se válido, False caso contrário.</returns>
    public static bool IsValid(string role) => 
        !string.IsNullOrWhiteSpace(role) && ValidRoles.Contains(role);

    /// <summary>
    /// Obtém todos os roles válidos do sistema.
    /// </summary>
    /// <returns>Coleção de nomes de roles.</returns>
    public static IReadOnlyCollection<string> GetAllRoles() => ValidRoles;

    public override string ToString() => Value;
}
