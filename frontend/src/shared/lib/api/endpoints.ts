/**
 * Endpoints da API
 */

export const API_ENDPOINTS = {
  // Auth
  AUTH_ME: '/auth/me',
  AUTH_LOGIN: '/auth/login',
  AUTH_LOGOUT: '/auth/logout',

  // Users
  USERS: '/users',
  USER_BY_ID: (id: string) => `/users/${id}`,
  USER_STATUS: (id: string) => `/users/${id}/status`,

  // Balances
  BALANCES: '/balances',
  DAILY_BALANCES: '/balances/daily',

  // Transactions
  TRANSACTIONS: '/transactions',
  TRANSACTION_BY_ID: (id: string) => `/transactions/${id}`,

  // Categories
  CATEGORIES: '/categories',
  CATEGORY_BY_ID: (id: string) => `/categories/${id}`,
} as const;
