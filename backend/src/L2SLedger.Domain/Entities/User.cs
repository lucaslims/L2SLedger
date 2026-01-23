namespace L2SLedger.Domain.Entities;

/// <summary>
/// Entidade de usuário interno do sistema.
/// Representa o usuário após validação do Firebase.
/// Conforme ADR-001: Firebase é apenas IdP, dados de domínio no banco relacional.
/// </summary>
public class User : Entity
{
    public string FirebaseUid { get; private set; }
    public string Email { get; private set; }
    public string DisplayName { get; private set; }
    public bool EmailVerified { get; private set; }
    private readonly List<string> _roles = new();
    public IReadOnlyCollection<string> Roles => _roles.AsReadOnly();

    // Constructor for EF Core
    private User() : base() 
    {
        FirebaseUid = string.Empty;
        Email = string.Empty;
        DisplayName = string.Empty;
    }

    public User(string firebaseUid, string email, string displayName, bool emailVerified) 
        : base()
    {
        if (string.IsNullOrWhiteSpace(firebaseUid))
            throw new ArgumentException("Firebase UID não pode ser vazio", nameof(firebaseUid));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio", nameof(email));

        FirebaseUid = firebaseUid;
        Email = email;
        DisplayName = displayName ?? email;
        EmailVerified = emailVerified;
        
        // Usuário padrão começa com role Leitura
        _roles.Add("Leitura");
    }

    public void AddRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role não pode ser vazio", nameof(role));

        if (!_roles.Contains(role))
        {
            _roles.Add(role);
            UpdateTimestamp();
        }
    }

    public void RemoveRole(string role)
    {
        if (_roles.Contains(role))
        {
            _roles.Remove(role);
            UpdateTimestamp();
        }
    }

    public void UpdateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Nome não pode ser vazio", nameof(displayName));

        DisplayName = displayName;
        UpdateTimestamp();
    }

    public void VerifyEmail()
    {
        if (!EmailVerified)
        {
            EmailVerified = true;
            UpdateTimestamp();
        }
    }

    public bool HasRole(string role) => _roles.Contains(role);

    public bool IsAdmin() => _roles.Contains("Admin");
}
