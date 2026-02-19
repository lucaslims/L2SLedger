/**
 * Tipos comuns da API
 */

export interface PaginatedResponse<T> {
  data: T[];
  pagination: {
    currentPage: number;
    pageSize: number;
    totalPages: number;
    totalItems: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
}

export interface ApiResponse<T> {
  data: T;
  timestamp: string;
}

export type UserStatus = 'Pending' | 'Active' | 'Suspended' | 'Rejected';
export type TransactionType = 'Income' | 'Expense';
export type UserRole = 'Admin' | 'Leitura' | 'Financeiro';
