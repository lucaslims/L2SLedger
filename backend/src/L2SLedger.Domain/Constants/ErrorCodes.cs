namespace L2SLedger.Domain.Constants;

/// <summary>
/// Catálogo centralizado de códigos de erro do sistema.
/// Conforme ADR-021 - Modelo de Erros Semântico.
/// Movido para Domain para respeitar Clean Architecture: Domain define conceitos fundamentais.
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
    public const string AUTH_USER_NOT_FOUND = "AUTH_USER_NOT_FOUND";
    public const string AUTH_FIREBASE_ERROR = "AUTH_FIREBASE_ERROR";

    // VAL_ - Validação de dados
    public const string VAL_REQUIRED_FIELD = "VAL_REQUIRED_FIELD";
    public const string VAL_VALIDATION_FAILED = "VAL_VALIDATION_FAILED";
    public const string VAL_INVALID_VALUE = "VAL_INVALID_VALUE";
    public const string VAL_INVALID_FORMAT = "VAL_INVALID_FORMAT";
    public const string VAL_AMOUNT_NEGATIVE = "VAL_AMOUNT_NEGATIVE";
    public const string VAL_INVALID_DATE = "VAL_INVALID_DATE";
    public const string VAL_INVALID_RANGE = "VAL_INVALID_RANGE";
    public const string VAL_DUPLICATE_NAME = "VAL_DUPLICATE_NAME";
    public const string VAL_INVALID_REFERENCE = "VAL_INVALID_REFERENCE";
    public const string VAL_BUSINESS_RULE_VIOLATION = "VAL_BUSINESS_RULE_VIOLATION";

    // FIN_ - Regras financeiras e domínio
    public const string FIN_PERIOD_CLOSED = "FIN_PERIOD_CLOSED";
    public const string FIN_INSUFFICIENT_BALANCE = "FIN_INSUFFICIENT_BALANCE";
    public const string FIN_DUPLICATE_ENTRY = "FIN_DUPLICATE_ENTRY";
    public const string FIN_INVALID_TRANSACTION = "FIN_INVALID_TRANSACTION";
    public const string FIN_PERIOD_NOT_FOUND = "FIN_PERIOD_NOT_FOUND";
    public const string FIN_PERIOD_ALREADY_EXISTS = "FIN_PERIOD_ALREADY_EXISTS";
    public const string FIN_PERIOD_ALREADY_CLOSED = "FIN_PERIOD_ALREADY_CLOSED";
    public const string FIN_PERIOD_ALREADY_OPENED = "FIN_PERIOD_ALREADY_OPENED";
    public const string FIN_PERIOD_INVALID_OPERATION = "FIN_PERIOD_INVALID_OPERATION";
    public const string FIN_CATEGORY_NOT_FOUND = "FIN_CATEGORY_NOT_FOUND";
    public const string FIN_CATEGORY_INVALID_NAME = "FIN_CATEGORY_INVALID_NAME";
    public const string FIN_CATEGORY_NAME_TOO_LONG = "FIN_CATEGORY_NAME_TOO_LONG";
    public const string FIN_CATEGORY_INVALID_TYPE = "FIN_CATEGORY_INVALID_TYPE";
    public const string FIN_TRANSACTION_NOT_FOUND = "FIN_TRANSACTION_NOT_FOUND";
    public const string FIN_ADJUSTMENT_NOT_FOUND = "FIN_ADJUSTMENT_NOT_FOUND";
    public const string FIN_ADJUSTMENT_PERIOD_CLOSED = "FIN_ADJUSTMENT_PERIOD_CLOSED";
    public const string FIN_ADJUSTMENT_INVALID_ORIGINAL = "FIN_ADJUSTMENT_INVALID_ORIGINAL";
    public const string FIN_ADJUSTMENT_UNAUTHORIZED = "FIN_ADJUSTMENT_UNAUTHORIZED";
    public const string FIN_ADJUSTMENT_ALREADY_DELETED = "FIN_ADJUSTMENT_ALREADY_DELETED";

    // PERM_ - Permissões
    public const string PERM_ACCESS_DENIED = "PERM_ACCESS_DENIED";
    public const string PERM_ROLE_REQUIRED = "PERM_ROLE_REQUIRED";
    public const string PERM_INSUFFICIENT_PRIVILEGES = "PERM_INSUFFICIENT_PRIVILEGES";

    // USER_ - Gestão de usuários
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string USER_INVALID_STATUS_TRANSITION = "USER_INVALID_STATUS_TRANSITION";
    public const string USER_STATUS_REQUIRED = "USER_STATUS_REQUIRED";
    public const string USER_STATUS_REASON_REQUIRED = "USER_STATUS_REASON_REQUIRED";
    public const string USER_STATUS_REASON_TOO_LONG = "USER_STATUS_REASON_TOO_LONG";
    public const string USER_INVALID_STATUS = "USER_INVALID_STATUS";
    public const string USER_CANNOT_MODIFY_OWN_STATUS = "USER_CANNOT_MODIFY_OWN_STATUS";
    public const string USER_CANNOT_REMOVE_OWN_ADMIN = "USER_CANNOT_REMOVE_OWN_ADMIN";
    public const string USER_LAST_ADMIN = "USER_LAST_ADMIN";
    public const string USER_ROLES_REQUIRED = "USER_ROLES_REQUIRED";
    public const string USER_ROLE_EMPTY = "USER_ROLE_EMPTY";
    public const string USER_INVALID_ROLE = "USER_INVALID_ROLE";

    // SYS_ - Erros de sistema
    public const string SYS_INTERNAL_ERROR = "SYS_INTERNAL_ERROR";
    public const string SYS_UNAVAILABLE = "SYS_UNAVAILABLE";
    public const string SYS_CONFIGURATION_ERROR = "SYS_CONFIGURATION_ERROR";

    // AUDIT_ - Auditoria
    public const string AUDIT_EVENT_NOT_FOUND = "AUDIT_EVENT_NOT_FOUND";

    // INT_ - Integrações externas
    public const string INT_FIREBASE_UNAVAILABLE = "INT_FIREBASE_UNAVAILABLE";
    public const string INT_DB_CONNECTION = "INT_DB_CONNECTION";
    public const string INT_EXTERNAL_SERVICE_ERROR = "INT_EXTERNAL_SERVICE_ERROR";

    // EXPORT_ - Exportações
    public const string EXPORT_NOT_FOUND = "EXPORT_NOT_FOUND";
    public const string EXPORT_DELETE_UNAUTHORIZED = "EXPORT_DELETE_UNAUTHORIZED";
    public const string EXPORT_UNAUTHORIZED = "EXPORT_UNAUTHORIZED";
    public const string EXPORT_NOT_COMPLETED = "EXPORT_NOT_COMPLETED";
    public const string EXPORT_NOT_READY = "EXPORT_NOT_READY";
    public const string EXPORT_INVALID_STATE = "EXPORT_INVALID_STATE";
    public const string EXPORT_INVALID_PARAMETERS = "EXPORT_INVALID_PARAMETERS";
}
