import type { ApiError } from '@/shared/types/errors.types';
import { ROUTES } from '@/shared/lib/utils/constants';

/**
 * Tratamento global de erros de autenticação
 * Baseado em ADR-021-A (Catálogo de Códigos de Erro)
 *
 * Redireciona usuário para páginas específicas baseado no código de erro
 */
export function handleAuthError(error: ApiError): void {
  const code = error.code;

  switch (code) {
    // Email não verificado → página de verificação
    case 'AUTH_EMAIL_NOT_VERIFIED':
      window.location.href = ROUTES.VERIFY_EMAIL;
      break;

    // Usuário aguardando aprovação → página pending
    case 'AUTH_USER_PENDING':
      window.location.href = ROUTES.PENDING_APPROVAL;
      break;

    // Usuário suspenso → página suspended
    case 'AUTH_USER_SUSPENDED':
      window.location.href = ROUTES.SUSPENDED;
      break;

    // Usuário rejeitado → página rejected
    case 'AUTH_USER_REJECTED':
      window.location.href = ROUTES.REJECTED;
      break;

    // Token inválido/expirado ou não autenticado → login
    case 'AUTH_INVALID_TOKEN':
    case 'AUTH_SESSION_EXPIRED':
    case 'AUTH_UNAUTHORIZED':
      window.location.href = ROUTES.LOGIN;
      break;

    // Demais erros de auth → login por segurança
    case 'AUTH_USER_INACTIVE':
    case 'AUTH_USER_NOT_FOUND':
    case 'AUTH_FIREBASE_ERROR':
      console.error('Auth error:', error);
      window.location.href = ROUTES.LOGIN;
      break;

    default:
      // Não é erro de autenticação, deixar tratamento para camada superior
      console.error('Unhandled error:', error);
  }
}

/**
 * Mapear mensagens de erro amigáveis por código
 */
export const ERROR_MESSAGES: Record<string, string> = {
  // Auth
  AUTH_INVALID_TOKEN: 'Sessão inválida. Faça login novamente.',
  AUTH_EMAIL_NOT_VERIFIED: 'Por favor, verifique seu email antes de fazer login.',
  AUTH_SESSION_EXPIRED: 'Sua sessão expirou. Faça login novamente.',
  AUTH_UNAUTHORIZED: 'Você precisa fazer login para acessar esta página.',
  AUTH_USER_PENDING: 'Sua conta está aguardando aprovação do administrador.',
  AUTH_USER_SUSPENDED: 'Sua conta foi suspensa. Entre em contato com o suporte.',
  AUTH_USER_REJECTED: 'Seu cadastro não foi aprovado.',
  AUTH_USER_INACTIVE: 'Sua conta está inativa.',
  AUTH_USER_NOT_FOUND: 'Usuário não encontrado.',
  AUTH_FIREBASE_ERROR: 'Erro no serviço de autenticação. Tente novamente mais tarde.',

  // Validation
  VAL_REQUIRED_FIELD: 'Campo obrigatório não preenchido.',
  VAL_VALIDATION_FAILED: 'Dados inválidos. Verifique os campos.',
  VAL_INVALID_VALUE: 'Valor inválido para este campo.',
  VAL_INVALID_FORMAT: 'Formato inválido.',
  VAL_AMOUNT_NEGATIVE: 'O valor não pode ser negativo.',
  VAL_INVALID_DATE: 'Data inválida.',
  VAL_INVALID_RANGE: 'Intervalo de datas inválido.',
  VAL_DUPLICATE_NAME: 'Este nome já está em uso.',
  VAL_INVALID_REFERENCE: 'Referência inválida.',
  VAL_BUSINESS_RULE_VIOLATION: 'Regra de negócio violada.',

  // Financial
  FIN_CATEGORY_NOT_FOUND: 'Categoria não encontrada.',
  FIN_CATEGORY_HAS_TRANSACTIONS: 'Esta categoria possui lançamentos e não pode ser excluída.',
  FIN_TRANSACTION_NOT_FOUND: 'Lançamento não encontrado.',
  FIN_PERIOD_CLOSED: 'Período fechado. Não é possível editar/excluir este lançamento.',
  FIN_AMOUNT_EXCEEDS_LIMIT: 'Valor excede o limite permitido.',
  FIN_INVALID_TRANSACTION_TYPE: 'Tipo de lançamento inválido.',

  // Permissions
  PERM_ACCESS_DENIED: 'Acesso negado.',
  PERM_ADMIN_REQUIRED: 'Esta ação requer permissões de administrador.',
  PERM_INSUFFICIENT_PRIVILEGES: 'Você não tem privilégios suficientes para realizar esta ação.',
  PERM_WRITE_REQUIRED: 'Você não tem permissão para editar dados.',

  // Users
  USER_NOT_FOUND: 'Usuário não encontrado.',
  USER_INVALID_STATUS_TRANSITION: 'Transição de status inválida.',
  USER_STATUS_REQUIRED: 'O status é obrigatório.',
  USER_STATUS_REASON_REQUIRED: 'O motivo da alteração de status é obrigatório.',
  USER_STATUS_REASON_TOO_LONG: 'O motivo excede o tamanho máximo permitido.',
  USER_INVALID_STATUS: 'Valor de status inválido.',
  USER_CANNOT_MODIFY_OWN_STATUS: 'Você não pode alterar seu próprio status.',
  USER_CANNOT_REMOVE_OWN_ADMIN: 'Você não pode remover sua própria permissão de admin.',
  USER_LAST_ADMIN: 'Não é possível remover o último administrador do sistema.',
  USER_ROLES_REQUIRED: 'O usuário deve ter pelo menos uma role.',
  USER_ROLE_EMPTY: 'A role não pode estar vazia.',
  USER_INVALID_ROLE: 'Role inválida especificada.',

  // System
  SYS_INTERNAL_ERROR: 'Erro interno do servidor. Tente novamente mais tarde.',
  SYS_SERVICE_UNAVAILABLE: 'Serviço temporariamente indisponível.',
  SYS_DATABASE_ERROR: 'Erro de banco de dados.',

  // Integrations
  INT_FIREBASE_UNAVAILABLE: 'Serviço Firebase temporariamente indisponível.',
  INT_EXTERNAL_API_ERROR: 'Erro ao comunicar com serviço externo.',

  // Export
  EXPORT_INVALID_FORMAT: 'Formato de exportação inválido.',
  EXPORT_TOO_MANY_RECORDS: 'Muitos registros para exportar. Use filtros.',
  EXPORT_GENERATION_FAILED: 'Erro ao gerar arquivo de exportação.',

  // Generic
  GENERIC_ERROR: 'Erro desconhecido. Tente novamente.',
};

/**
 * Obter mensagem amigável para um código de erro
 */
export function getErrorMessage(code: string): string {
  return ERROR_MESSAGES[code] || ERROR_MESSAGES.GENERIC_ERROR;
}
