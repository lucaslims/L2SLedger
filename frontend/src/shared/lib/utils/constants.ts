/**
 * Constantes globais do sistema
 */

export const APP_NAME = 'L2SLedger';
export const APP_VERSION = '1.0.0';

export const ROUTES = {
  HOME: '/',
  LOGIN: '/login',
  REGISTER: '/register',
  VERIFY_EMAIL: '/verify-email',
  PENDING_APPROVAL: '/pending-approval',
  SUSPENDED: '/suspended',
  REJECTED: '/rejected',
  DASHBOARD: '/dashboard',
  TRANSACTIONS: '/transactions',
  TRANSACTIONS_NEW: '/transactions/new',
  TRANSACTIONS_EDIT: '/transactions/:id/edit',
  CATEGORIES: '/categories',
  CATEGORIES_NEW: '/categories/new',
  CATEGORIES_EDIT: '/categories/:id/edit',
  ADMIN: '/admin',
  ADMIN_USERS: '/admin/users',
  ADMIN_USER_DETAIL: '/admin/users/:id',
} as const;

export const USER_STATUS = {
  PENDING: 'Pending',
  ACTIVE: 'Active',
  SUSPENDED: 'Suspended',
  REJECTED: 'Rejected',
} as const;

export const USER_STATUS_LABELS = {
  [USER_STATUS.PENDING]: 'Pendente',
  [USER_STATUS.ACTIVE]: 'Ativo',
  [USER_STATUS.SUSPENDED]: 'Suspenso',
  [USER_STATUS.REJECTED]: 'Rejeitado',
} as const;

export const TRANSACTION_TYPE = {
  INCOME: 'Income',
  EXPENSE: 'Expense',
} as const;

export const TRANSACTION_TYPE_LABELS = {
  [TRANSACTION_TYPE.INCOME]: 'Receita',
  [TRANSACTION_TYPE.EXPENSE]: 'Despesa',
} as const;

export const ROLES = {
  ADMIN: 'Admin',
  LEITURA: 'Leitura',
  FINANCEIRO: 'Financeiro',
} as const;

export const PAGINATION = {
  DEFAULT_PAGE_SIZE: 20,
  PAGE_SIZE_OPTIONS: [10, 20, 50, 100],
} as const;

export const QUERY_KEYS = {
  AUTH: 'auth',
  USER: 'user',
  USERS: 'users',
  BALANCES: 'balances',
  DAILY_BALANCES: 'daily-balances',
  TRANSACTIONS: 'transactions',
  TRANSACTION: 'transaction',
  CATEGORIES: 'categories',
  CATEGORY: 'category',
} as const;

export const STORAGE_KEYS = {
  THEME: 'l2sledger:theme',
} as const;
