namespace L2SLedger.API.Contracts;

/// <summary>
/// Catálogo centralizado de códigos de erro da API.
/// Conforme ADR-021 - Modelo de Erros Semântico.
/// </summary>
public static class ErrorCodes
{
    // AUTH_ - Autenticação e autorização
    public const string AUTH_INVALID_TOKEN = "AUTH_INVALID_TOKEN";
    public const string AUTH_EMAIL_NOT_VERIFIED = "AUTH_EMAIL_NOT_VERIFIED";
    public const string AUTH_SESSION_EXPIRED = "AUTH_SESSION_EXPIRED";
    public const string AUTH_UNAUTHORIZED = "AUTH_UNAUTHORIZED";
    public const string AUTH_USER_PENDING = "AUTH_USER_PENDING";
    public const string AUTH_USER_SUSPENDED = "AUTH_USER_SUSPENDED";
    public const string AUTH_USER_REJECTED = "AUTH_USER_REJECTED";
    public const string AUTH_USER_INACTIVE = "AUTH_USER_INACTIVE";

    // VAL_ - Validação de dados
    public const string VAL_REQUIRED_FIELD = "VAL_REQUIRED_FIELD";
    public const string VAL_INVALID_FORMAT = "VAL_INVALID_FORMAT";
    public const string VAL_AMOUNT_NEGATIVE = "VAL_AMOUNT_NEGATIVE";
    public const string VAL_INVALID_DATE = "VAL_INVALID_DATE";
    public const string VAL_INVALID_RANGE = "VAL_INVALID_RANGE";

    // FIN_ - Regras financeiras e domínio
    public const string FIN_PERIOD_CLOSED = "FIN_PERIOD_CLOSED";
    public const string FIN_INSUFFICIENT_BALANCE = "FIN_INSUFFICIENT_BALANCE";
    public const string FIN_DUPLICATE_ENTRY = "FIN_DUPLICATE_ENTRY";
    public const string FIN_INVALID_TRANSACTION = "FIN_INVALID_TRANSACTION";

    // PERM_ - Permissões
    public const string PERM_ACCESS_DENIED = "PERM_ACCESS_DENIED";
    public const string PERM_ROLE_REQUIRED = "PERM_ROLE_REQUIRED";
    public const string PERM_INSUFFICIENT_PRIVILEGES = "PERM_INSUFFICIENT_PRIVILEGES";

    // USER_ - Gestão de usuários
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string USER_INVALID_STATUS_TRANSITION = "USER_INVALID_STATUS_TRANSITION";
    public const string USER_STATUS_REASON_REQUIRED = "USER_STATUS_REASON_REQUIRED";
    public const string USER_CANNOT_MODIFY_OWN_STATUS = "USER_CANNOT_MODIFY_OWN_STATUS";

    // SYS_ - Erros de sistema
    public const string SYS_INTERNAL_ERROR = "SYS_INTERNAL_ERROR";
    public const string SYS_UNAVAILABLE = "SYS_UNAVAILABLE";
    public const string SYS_CONFIGURATION_ERROR = "SYS_CONFIGURATION_ERROR";

    // INT_ - Integrações externas
    public const string INT_FIREBASE_UNAVAILABLE = "INT_FIREBASE_UNAVAILABLE";
    public const string INT_DB_CONNECTION = "INT_DB_CONNECTION";
    public const string INT_EXTERNAL_SERVICE_ERROR = "INT_EXTERNAL_SERVICE_ERROR";
}
